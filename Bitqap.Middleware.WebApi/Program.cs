
var builder = WebApplication.CreateBuilder(args);
Logger logger = LogManager.GetCurrentClassLogger();

// Add services to the container.
//with ConfigureApiBehaviorOptions added custom model state error handling
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var modelState = actionContext.ModelState.Values;
        var errorList = modelState.Select(x => x.Errors.Select(y => y.ErrorMessage)).Select(c => c.FirstOrDefault()).ToList();
        var errorDetails = new ErrorDetails
        { Message = "Invalid request body", StatusCode = "VALIDATION", Errors = errorList };
        logger.Log(NLog.LogLevel.Error, $"Validation exception --> {System.Text.Json.JsonSerializer.Serialize(errorDetails)}");
        return new BadRequestObjectResult(errorDetails);
    };
})
    .AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                    }
                },
                new string[] {}
        }
    });
});

//get settings from appsettings.json file
var apiSettings = builder.Configuration.GetSection("ApiSettings");
var jwtSettings = builder.Configuration.GetSection("JsonWebTokenKeys");
builder.Services.Configure<ApiSettings>(apiSettings);
builder.Services.Configure<JwtSettings>(jwtSettings);

//Configuration of ServiceContext
var servicepoint = builder.Services.BuildServiceProvider();
var jwtAuthSettings = servicepoint.GetService<IOptions<JwtSettings>>();
var apiInjectedSettings = servicepoint.GetService<IOptions<ApiSettings>>();

//dependency injection
IUserDataAccess userDataAccess = new UserDataAccess(apiInjectedSettings.Value.DbConnection);
IAccountDataAccess accountDataAccess = new AccountDataAccess(apiInjectedSettings.Value.DbConnection);
IMessagePayloadDataAccess msgPayloadDataAccess = new MessagePayloadDataAccess(apiInjectedSettings.Value.DbConnection);
IMappingExtension mappingExtension = new MappingExtension();
IUserService userService = new UserService(userDataAccess, mappingExtension, apiInjectedSettings.Value);
IMessagePayloadService msgPayloadService = new MessagePayloadService(msgPayloadDataAccess);
SocketClient socketClient = SocketClient.GetInstance(apiInjectedSettings.Value, msgPayloadService);
IAccountService accountService = new AccountService(accountDataAccess, socketClient, userService, msgPayloadService, mappingExtension, apiInjectedSettings.Value);

builder.Services.AddSingleton<IMessagePayloadDataAccess>(msgPayloadDataAccess);
builder.Services.AddSingleton<IUserService>(userService);
builder.Services.AddSingleton<IAccountService>(accountService);
builder.Services.AddSingleton<IMappingExtension>(mappingExtension);

//configure jwt auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = jwtAuthSettings.Value.ValidateAudience,
        //ValidAudience = jwtAuthSettings.Value.ValidAudience,
        ValidateIssuer = jwtAuthSettings.Value.ValidateIssuer,
        //ValidIssuer = jwtAuthSettings.Value.ValidIssuer,

        ValidateIssuerSigningKey = jwtAuthSettings.Value.ValidateIssuerSigningKey,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthSettings.Value.IssuerSigningKey)),

        ValidateLifetime = jwtAuthSettings.Value.ValidateLifetime, //validate the expiration and not before values in the token

        ClockSkew = TimeSpan.FromMinutes(15) //x minute tolerance for the expiration date
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(new ErrorDetails
            {
                Message = string.IsNullOrEmpty(context.ErrorDescription) ? "Invalid token!" : context.ErrorDescription,
                StatusCode = "UNAUTHRIZED"
            }.ToString());
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
        JwtBearerDefaults.AuthenticationScheme);

    defaultAuthorizationPolicyBuilder =
        defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();

    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.ConfigureExceptionHandler(logger);

#pragma warning disable CS0618 // Type or member is obsolete
logger.Log(NLog.LogLevel.Info, "WebApi started", default(Exception));
#pragma warning restore CS0618 // Type or member is obsolete

app.Run();


namespace Bitqap.Middleware.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly ApiSettings _settings;
        private readonly JwtSettings _jwtSettings;
        private readonly IUserService _userService;
        private readonly IMappingExtension _mappingExtension;
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public UserController(IOptions<ApiSettings> settings, IOptions<JwtSettings> jwtSettings, IUserService userService, IMappingExtension mappingExtension)
        {
            this._settings = settings.Value;
            this._jwtSettings = jwtSettings.Value;
            _userService = userService;
            _mappingExtension = mappingExtension;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> RegisterUser([FromBody] RegisterUserRequest req)
        {
            _logger.Log(NLog.LogLevel.Info, "Request received 4 create new User", default(Exception));
            User user = _mappingExtension.Map<RegisterUserRequest, User>(req);
            var result = await _userService.RegisterNewUser(user);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            _logger.Log(NLog.LogLevel.Info, "Request received login", default(Exception));
            var existedUser = await _userService.LoginUser(loginRequest);
            existedUser.Token = JwtHelper.GenerateToken(existedUser, _jwtSettings);
            return Ok(existedUser);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut()]
        public async Task<ActionResult> UpdateUser([FromBody] User entity)
        {
            _logger.Log(NLog.LogLevel.Info, "Request received udate user", default(Exception));
            if (entity == null) throw new BitqapBusinessException("Empty request body", "EMPTY_BODY");

            var token = Request.Headers.Authorization.ToString();
            var requestedUser = JwtHelper.GetUserFromBearerToken(token);
            if (!string.Equals(entity.Username, requestedUser.Username, StringComparison.InvariantCultureIgnoreCase))
                throw new BitqapBusinessException("Not allowed update this user", "NOT_ALLOWED");
            else await _userService.UpdateUser(entity);

            return Ok(entity);
        }
    }
}

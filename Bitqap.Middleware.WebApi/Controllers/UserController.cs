
namespace Bitqap.Middleware.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApiSettings _settings;
        private readonly JwtSettings _jwtSettings;
        private readonly IUserService _userService;
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public UserController(IOptions<ApiSettings> settings, IOptions<JwtSettings> jwtSettings, IUserService userService)
        {
            this._settings = settings.Value;
            this._jwtSettings = jwtSettings.Value;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> RegisterUser([FromBody] RegisterUserRequest req)
        {
            _logger.Log(NLog.LogLevel.Info, "Request received 4 create new User", default(Exception));
            var mapper = new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<RegisterUserRequest,User>()));
            User user = mapper.Map<User>(req);
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
    }
}

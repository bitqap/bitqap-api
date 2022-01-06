
using Bitqap.Middleware.Business.Services;

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

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var token = Request.HttpContext.Request?.Headers["Authorization"];
            var user = JwtHelper.GetUserFromBearerToken(token);
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> RegisterUser([FromBody]Bitqap.Middleware.Entity.ApiEntity.User user)
        {
            _logger.Log(NLog.LogLevel.Info, "Request received 4 create new User", default(Exception));
            var result = await _userService.RegisterNewUser(user);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = new User { Username="sulkjayev",
            Id=1, Lastname="Hijran", Firstname="S", Password="121343", RegisterDate=DateTime.Now
            };

            user.Token = JwtHelper.GenerateToken(user, _jwtSettings);

            return Ok(user);
        }
    }
}

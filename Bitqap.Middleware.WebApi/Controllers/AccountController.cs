namespace Bitqap.Middleware.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AccountController : ControllerBase
    {
        private readonly ApiSettings _settings;
        private readonly JwtSettings _jwtSettings;
        private readonly IAccountService _accountService;
        private readonly IMappingExtension _mappingExtension;
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public AccountController(IOptions<ApiSettings> settings, IOptions<JwtSettings> jwtSettings, IAccountService accountService, IMappingExtension mappingExtension)
        {
            this._settings = settings.Value;
            this._jwtSettings = jwtSettings.Value;
            _accountService = accountService;
            _mappingExtension = mappingExtension;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult> CreateAccount([FromBody] Account req)
        {
            _logger.Log(NLog.LogLevel.Info, "Request received 4 create new Account", default(Exception));
            if (req == null) throw new BitqapBusinessException("Empty request body", "EMPTY_BODY");

            var token = Request.Headers.Authorization.ToString();
            var requestedUser = JwtHelper.GetUserFromBearerToken(token);
            if (req.UserId != requestedUser.Id)
                throw new BitqapBusinessException("Not allowed", "NOT_ALLOWED");
            var result = await _accountService.CreateAccount(req);
            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("request-balance/{accountKey}")]
        public async Task<ActionResult> RequestBalance(string accountKey)
        {
            _logger.Log(NLog.LogLevel.Info, "Request received RequestBalance", default(Exception));

            var token = Request.Headers.Authorization.ToString();
            var requestedUser = JwtHelper.GetUserFromBearerToken(token);
            var result = await _accountService.RequestAccountBalanceByAccountKey(accountKey, requestedUser.Id);
            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-balance/{requestKey}")]
        public async Task<ActionResult> GetBalance(string requestKey)
        {
            _logger.Log(NLog.LogLevel.Info, "Request received GetBalance", default(Exception));

            var token = Request.Headers.Authorization.ToString();
            var requestedUser = JwtHelper.GetUserFromBearerToken(token);
            var result = await _accountService.GetRequestedBalance(requestKey, requestedUser.Id);
            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("request-transactions/{accountKey}")]
        public async Task<ActionResult> RequestTransactions(string accountKey)
        {
            _logger.Log(NLog.LogLevel.Info, "Request received RequestTransactions", default(Exception));

            var token = Request.Headers.Authorization.ToString();
            var requestedUser = JwtHelper.GetUserFromBearerToken(token);
            var result = await _accountService.RequestTransactionsByAccountKey(accountKey, requestedUser.Id);
            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-transactions/{requestKey}")]
        public async Task<ActionResult> GetTransactions(string requestKey)
        {
            _logger.Log(NLog.LogLevel.Info, "Request received GetTransactions", default(Exception));

            var token = Request.Headers.Authorization.ToString();
            var requestedUser = JwtHelper.GetUserFromBearerToken(token);
            var result = await _accountService.GetRequestedTransactions(requestKey, requestedUser.Id);
            return Ok(result);
        }
    }
}

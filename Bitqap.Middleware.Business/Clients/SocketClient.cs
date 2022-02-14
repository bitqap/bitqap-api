using Bitqap.Middleware.Business.Services;
using Bitqap.Middleware.Business.Utils;
using Bitqap.Middleware.Entity.ApiEntity;
using Bitqap.Middleware.Entity.SocketEntity;
using NLog;
using WebsocketsSimple.Client;
using WebsocketsSimple.Client.Events.Args;
using WebsocketsSimple.Client.Models;

namespace Bitqap.Middleware.Business.Clients
{
    public sealed class SocketClient
    {
        readonly ApiSettings _apiSettings;
        readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IWebsocketClient _wsClient;
        private readonly IMessagePayloadService _msgPayloadService;
        private static readonly object _lock = new();
        private static SocketClient _instance;
        public delegate string ReceiverDelegate(string msg);
        public ReceiverDelegate RcDlg { get; set; }

        SocketClient(ApiSettings apiSettings, IMessagePayloadService msgPayloadService)
        {
            _apiSettings = apiSettings;
            _msgPayloadService = msgPayloadService;
            _wsClient = new WebsocketClient(new ParamsWSClient
            {
                Uri = _apiSettings.SocketHostUrl,
                Port = _apiSettings.SocketHostPort,
                IsWebsocketSecured = false,
                ReceiveBufferSize = 1024000,
                SendBufferSize = 1024000
            });
            ConnectToSocket();
        }

        public static SocketClient GetInstance(ApiSettings apiSettings, IMessagePayloadService msgPayloadService)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SocketClient(apiSettings, msgPayloadService);
                    }
                }
            }
            return _instance;
        }

        private async Task ConnectToSocket()
        {
            _wsClient.ConnectionEvent += OnConnectionEvent;
            _wsClient.ErrorEvent += OnErrorEvent;
            _wsClient.MessageEvent += OMessageEvent;
            RcDlg = (string msg) => msg;
            await _wsClient.ConnectAsync();
        }

        private async Task OnErrorEvent(object sender, WSErrorClientEventArgs args)
        {
            _logger.Log(LogLevel.Error, $"OnErrorEvent: {args.Id}", args.Exception);
        }

        private async Task OnConnectionEvent(object sender, WSConnectionClientEventArgs args)
        {
            _logger.Log(LogLevel.Info, "Connected to socket", default(Exception));
        }

        public async Task OMessageEvent(object sender, WSMessageClientEventArgs args)
        {
            _logger.Log(LogLevel.Debug, "Message received", default(Exception));
            _logger.Log(LogLevel.Debug, args.Message, default(Exception));
            if (!string.IsNullOrEmpty(args.Message))
            {
                if (args.Message.Contains("responseID"))
                {
                    var cmnResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<CommonSocketResponse>(args.Message);
                    var msgPyld = new MsgPayload
                    {
                        Direction = MsgDirection.RECEIVED,
                        MethodName = cmnResponse.command,
                        Payload = args.Message,
                        RequestKey = cmnResponse.responseID
                    };

                    await _msgPayloadService.AddMessagePayload(msgPyld);
                }
            }
            RcDlg.Invoke(args.Message);
        }

        public async Task SendMessage(MsgPayload msgPyld)
        {
            if (!_wsClient.IsRunning) throw new BitqapBusinessException("No connection to socket", "SOCKET_NOT_CONNECTED");
            await _wsClient.SendToServerRawAsync(msgPyld.Payload);
            await _msgPayloadService.AddMessagePayload(msgPyld);
            _logger.Log(LogLevel.Debug, $"Message Sent: {msgPyld.Payload}", default(Exception));
        }
    }
}

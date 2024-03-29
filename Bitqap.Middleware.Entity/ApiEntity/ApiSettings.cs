﻿namespace Bitqap.Middleware.Entity.ApiEntity
{
    public class ApiSettings
    {
        public string? ApiKey { get; set; }
        public string? SocketHostUrl { get; set; }
        public int SocketHostPort { get; set; }
        public string? DbConnection { get; set; }
        public int KeySizeInBits { get; set; }
    }
}

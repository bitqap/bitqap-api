namespace Bitqap.Middleware.WebApi.Extensions
{
    public static class ExceptionMiddlewareExtension
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, Logger logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        if (contextFeature.Error is BitqapBusinessException)
                        {
                            logger.Log(NLog.LogLevel.Error, $"Business exception --> {contextFeature.Error}");
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            await context.Response.WriteAsync(new ErrorDetails()
                            {
                                StatusCode = ((BitqapBusinessException)contextFeature.Error).ErrorCode,
                                Message = ((BitqapBusinessException)contextFeature.Error).Message
                            }.ToString());;
                        }
                        else
                        {
                            var trackingID = Guid.NewGuid().ToString("N");
                            logger.Log(NLog.LogLevel.Error, $"Technical error, trackingID: {trackingID} --> {contextFeature.Error}");
                            await context.Response.WriteAsync(new ErrorDetails()
                            {
                                StatusCode = context.Response.StatusCode.ToString(),
                                Message = $"Internal Server Error, trackingID: {trackingID}"
                            }.ToString());
                        }
                    }
                });
            });
        }
    }
}

namespace ASP.NET_Hands_on.Middlewares
{
    public class ApiKeyCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEY_HEADER_NAME = "X-Api-Key";

        public ApiKeyCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            //get the api key from appsettings file
            var validApiKey = configuration.GetValue<string>("ApiKeyConfig");

            //check if user sends api key with the request
            if (!context.Request.Headers.TryGetValue(APIKEY_HEADER_NAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Missing API Key");
                return;
            }

            //if user do, check validation of the api key
            if (!validApiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid API Key");
                return;
            }
            
            //process to next in pipeline
            await _next(context);
        }
    }
}

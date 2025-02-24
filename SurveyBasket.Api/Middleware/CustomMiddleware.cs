namespace SurveyBasket.Api.Middleware
{
	public class CustomMiddleware
	{
		private readonly ILogger<CustomMiddleware> _logger;
		private readonly RequestDelegate _next;

		public CustomMiddleware(ILogger<CustomMiddleware> logger, RequestDelegate next)
		{
			_logger = logger;
			_next = next;
		}

		//public async Task InvokeAsync(HttpContext context)
		//{
		//	var endPoint = context.GetEndpoint();
		//	Console.WriteLine($"Endpoint: {endPoint}");
		//	_logger.LogInformation("Processing request");
		//	await _next(context);
		//	_logger.LogError("Processing response");
		//}
	}
}

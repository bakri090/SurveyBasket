using Microsoft.AspNetCore.Mvc;

namespace SurveyBasket.Api.Middleware
{
	public static class CustomMiddlewareExtension 
	{
		public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<CustomMiddleware>();
		} 
	}
}

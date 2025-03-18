using Hangfire;
using HangfireBasicAuthenticationFilter;
using Serilog;
using SurveyBasket.Api;
using SurveyBasket.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDependencies(builder.Configuration);

builder.Services.AddDistributedMemoryCache();

builder.Host.UseSerilog((context, configuration) =>
	configuration.ReadFrom.Configuration(context.Configuration)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseHangfireDashboard("/jobs",
	new DashboardOptions
	{
		Authorization = [
			new HangfireCustomBasicAuthenticationFilter{
				User = app.Configuration.GetValue<string>("HangFireSettings:Username"),
				Pass = app.Configuration.GetValue<string>("HangFireSettings:Password")
			}
			],
		DashboardTitle = "Survey Basket",
		//IsReadOnlyFunc = (DashboardContext context) => true
	});
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var scope = scopeFactory.CreateScope();
var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

RecurringJob.AddOrUpdate("SendNewPollsNotification", () => notificationService.SendNewPollsNotification(null), Cron.Daily);

app.UseCors("WithOrigin");

app.UseAuthorization();

//add caching after Cors and Authorization
app.UseResponseCaching();

app.MapControllers();

app.UseExceptionHandler();

app.Run();

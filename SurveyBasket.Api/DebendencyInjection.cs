using MapsterMapper;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SurveyBasket.Api.Persistence;
using System.Reflection;

namespace SurveyBasket.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddDependencies(this IServiceCollection services,IConfiguration configuration)
	{
		// Add services to the container.

		services.AddControllers();
		// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
		services
			.AddOpenApi()
			.AddMapsterConfig()
			.AddSwaggerServices()
			.AddFluentValidationConfig();
		configuration.AddDatabase(services);
		// Services
		services.AddScoped<IPollService, PollsService>();
		

		return services;
	}
	// Swagger
	public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
	{
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();
		return services;
	}
	// AddMapster
	public static IServiceCollection AddMapsterConfig(this IServiceCollection services)
	{
		var mappingConfig = TypeAdapterConfig.GlobalSettings;
		mappingConfig.Scan(Assembly.GetExecutingAssembly());

		services.AddSingleton<IMapper>(new Mapper(mappingConfig));
		return services;
	}
	//Validator
	// [1]services.AddScoped<IValidator<CreatePollRequest>, CreatePollRequestValidator>();
	// [2] 
	public static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
	{
		services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly()).AddFluentValidationAutoValidation();
		return services;
	}
	public static IConfiguration AddDatabase(this IConfiguration configuration,IServiceCollection services)
	{
		var connection = configuration.GetConnectionString("DefCon") ??
		throw new InvalidOperationException("Connection string 'DefCon' not Found");

		services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));

		return configuration;
	}

}

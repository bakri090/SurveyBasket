using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Settings;
using System.Reflection;
using System.Text;
using FluentValidation.AspNetCore;
using MapsterMapper;
using Hangfire;
using SurveyBasket.Api.Services;


namespace SurveyBasket.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddDependencies(this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddControllers();


		services.AddAuthConfig(configuration);


		var connectionString = configuration.GetConnectionString("DefCon") ??
			throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(connectionString));
		var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

		services.AddCors(op =>
		op.AddDefaultPolicy(builder =>
			builder
				.AllowAnyHeader()
				.AllowAnyMethod()
				.WithOrigins(allowedOrigins!)
			)
		);

		services
			.AddSwaggerServices()
			.AddMapsterConfig()
			.AddFluentValidationConfig();

		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<IEmailSender, EmailService>();
		services.AddScoped<IPollService, PollService>();
		services.AddScoped<IQuestionService, QuestionService>();
		services.AddScoped<IVoteService, VoteService>();
		services.AddScoped<IResultService, ResultService>();
		services.AddScoped<ICacheService, CacheService>();
		services.AddScoped<INotificationService, NotificationService>();
		services.AddScoped<IUserService, UserService>();
		services.AddExceptionHandler<GlobalExceptionHandler>();
		services.AddProblemDetails();
		services.AddHttpContextAccessor();
		services.AddBackgroundJobConfig(configuration);

		services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));

		return services;
	}

	private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
	{
		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();

		return services;
	}

	private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
	{
		var mappingConfig = TypeAdapterConfig.GlobalSettings;
		mappingConfig.Scan(Assembly.GetExecutingAssembly());

		services.AddSingleton<IMapper>(new Mapper(mappingConfig));

		return services;
	}

	private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
	{
		services
			.AddFluentValidationAutoValidation()
			.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

		return services;
	}

	private static IServiceCollection AddAuthConfig(this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddIdentity<ApplicationUser, IdentityRole>()
		  .AddEntityFrameworkStores<ApplicationDbContext>()
		  .AddDefaultTokenProviders();


		services.AddSingleton<IJwtProvider, JwtProvider>();

		//services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
		services.AddOptions<JwtOptions>()
			.BindConfiguration(JwtOptions.SectionName)
			.ValidateDataAnnotations()
			;

		var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(o =>
		{
			o.SaveToken = true;
			o.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)),
				ValidIssuer = jwtSettings?.Issuer,
				ValidAudience = jwtSettings?.Audience
			};
		});

		services.Configure<IdentityOptions>(options =>
		{
			options.Password.RequiredLength = 8;
			options.SignIn.RequireConfirmedEmail = true;
			options.User.RequireUniqueEmail = true;
		});
		return services;
	}
	private static IServiceCollection AddBackgroundJobConfig(this IServiceCollection services,
		IConfiguration configuration)
	{
		// Add Hangfire services.
		services.AddHangfire(config => config
			.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
			.UseSimpleAssemblyNameTypeSerializer()
			.UseRecommendedSerializerSettings()
			.UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

		// Add the processing server as IHostedService
		services.AddHangfireServer();

		return services;
	}
}
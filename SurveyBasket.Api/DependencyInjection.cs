using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Settings;
using System.Reflection;
using System.Text;
using FluentValidation.AspNetCore;
using MapsterMapper;
using Hangfire;
using SurveyBasket.Api.Health;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Asp.Versioning;
using SurveyBasket.Api.OpenApiTransformer;
using Asp.Versioning.ApiExplorer;


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
			.AddMapsterConfig()
			.AddFluentValidationConfig();

		services.AddScoped<IAuthServices, AuthServices>();
		services.AddScoped<IEmailSender, EmailServices>();
		services.AddScoped<IPollServices, PollServices>();
		services.AddScoped<IQuestionServices, QuestionServices>();
		services.AddScoped<IVoteServices, VoteServices>();
		services.AddScoped<IResultServices, ResultServices>();
		services.AddScoped<ICacheServices, CacheServices>();
		services.AddScoped<INotificationServices, NotificationServices>();
		services.AddScoped<IUserServices, UserServices>();
		services.AddScoped<IRoleServices, RoleServices>();

		services.AddExceptionHandler<GlobalExceptionHandler>();
		services.AddProblemDetails();
		services.AddHttpContextAccessor();
		services.AddBackgroundJobConfig(configuration);

		services.Configure<MailSettings>(configuration.GetSection(nameof (MailSettings)));

		services.AddHealthChecks()
			.AddDbContextCheck<ApplicationDbContext>("database")
			.AddHangfire(op => { op.MinimumAvailableServers = 1;})
			.AddCheck<MailProviderHealthCheck>(name: "mail service");

		services.AddRateLimitConfig();

		services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = new ApiVersion(1.0);
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.ReportApiVersions = true;

			options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
		}).
		AddApiExplorer(options =>
		{
			options.GroupNameFormat = "'v'V";
			options.SubstituteApiVersionInUrl = true;
		});

		services.AddEndpointsApiExplorer()
			.AddOpenApiConfig();

		return services;
	}

	//private static IServiceCollection AddSwaggerservices(this IServiceCollection services)
	//{
	//	// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
	//	services.AddEndpointsApiExplorer();
	//	services.AddSwaggerGen();

	//	return services;
	//}
	private static IServiceCollection AddOpenApiConfig(this IServiceCollection services)
	{
		var serviceProvider = services.BuildServiceProvider();
		var provider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

		foreach (var description in provider.ApiVersionDescriptions)
		{
		services.AddOpenApi(description.GroupName, options =>
		{
			options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
			options.AddDocumentTransformer(new ApiVersioningTransformer(description));
		});
			
		}
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
		services.AddIdentity<ApplicationUser, ApplicationRole>()
		  .AddEntityFrameworkStores<ApplicationDbContext>()
		  .AddDefaultTokenProviders();

		services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
		services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

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

		// Add the processing server as IHostedservices
		services.AddHangfireServer();

		return services;
	}
	private static IServiceCollection AddRateLimitConfig(this IServiceCollection services)
	{
		services.AddRateLimiter(rateLimiterOption =>
		{
			rateLimiterOption.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			rateLimiterOption.AddPolicy("ipLimit", httpContext =>
			RateLimitPartition.GetFixedWindowLimiter(
				partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
				factory: _ => new FixedWindowRateLimiterOptions
				{
					PermitLimit = 2,
					Window = TimeSpan.FromSeconds(20)
				}));

			rateLimiterOption.AddPolicy("userLimit", httpContext =>
						RateLimitPartition.GetFixedWindowLimiter(
				partitionKey: httpContext.User.GetUserId(),
				factory: _ => new FixedWindowRateLimiterOptions
				{
					PermitLimit = 2,
					Window = TimeSpan.FromSeconds(20)
				}));

			rateLimiterOption.AddConcurrencyLimiter("concurrency", options =>
			{
				options.PermitLimit = 2;
				options.QueueLimit = 1;
				options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			});

			//rateLimiterOption.AddTokenBucketLimiter("token", op =>
			//{
			//	op.TokenLimit = 2;
			//	op.QueueLimit = 1;
			//	op.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			//	op.ReplenishmentPeriod = TimeSpan.FromSeconds(30);
			//	op.TokensPerPeriod = 2;
			//	op.AutoReplenishment = true;
			//});

			//rateLimiterOption.AddFixedWindowLimiter("fixed", op =>
			//{
			//	op.PermitLimit = 2;
			//	op.Window = TimeSpan.FromSeconds(20);
			//	op.QueueLimit = 1;
			//	op.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			//});

			//rateLimiterOption.AddSlidingWindowLimiter("sliding", op =>
			//{
			//	op.PermitLimit = 2;
			//	op.Window = TimeSpan.FromSeconds(20);
			//	op.QueueLimit = 1;
			//	op.SegmentsPerWindow = 2;
			//	op.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			//});
		});
		return services;

	}
}
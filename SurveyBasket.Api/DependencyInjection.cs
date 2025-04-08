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


namespace SurveyBasket.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddDependencies(this IServiceCollection Services,
		IConfiguration configuration)
	{
		Services.AddControllers();


		Services.AddAuthConfig(configuration);


		var connectionString = configuration.GetConnectionString("DefCon") ??
			throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

		Services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(connectionString));
		var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

		Services.AddCors(op =>
		op.AddDefaultPolicy(builder =>
			builder
				.AllowAnyHeader()
				.AllowAnyMethod()
				.WithOrigins(allowedOrigins!)
			)
		);

		Services
			.AddSwaggerServices()
			.AddMapsterConfig()
			.AddFluentValidationConfig();

		Services.AddScoped<IAuthServices, AuthServices>();
		Services.AddScoped<IEmailSender, EmailServices>();
		Services.AddScoped<IPollServices, PollServices>();
		Services.AddScoped<IQuestionServices, QuestionServices>();
		Services.AddScoped<IVoteServices, VoteServices>();
		Services.AddScoped<IResultServices, ResultServices>();
		Services.AddScoped<ICacheServices, CacheServices>();
		Services.AddScoped<INotificationServices, NotificationServices>();
		Services.AddScoped<IUserServices, UserServices>();
		Services.AddScoped<IRoleServices, RoleServices>();

		Services.AddExceptionHandler<GlobalExceptionHandler>();
		Services.AddProblemDetails();
		Services.AddHttpContextAccessor();
		Services.AddBackgroundJobConfig(configuration);

		Services.Configure<MailSettings>(configuration.GetSection(nameof (MailSettings)));

		Services.AddHealthChecks()
			.AddDbContextCheck<ApplicationDbContext>("database")
			.AddHangfire(op => { op.MinimumAvailableServers = 1;})
			.AddCheck<MailProviderHealthCheck>(name: "mail service");

		return Services;
	}

	private static IServiceCollection AddSwaggerServices(this IServiceCollection Services)
	{
		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		Services.AddEndpointsApiExplorer();
		Services.AddSwaggerGen();

		return Services;
	}

	private static IServiceCollection AddMapsterConfig(this IServiceCollection Services)
	{
		var mappingConfig = TypeAdapterConfig.GlobalSettings;
		mappingConfig.Scan(Assembly.GetExecutingAssembly());

		Services.AddSingleton<IMapper>(new Mapper(mappingConfig));

		return Services;
	}

	private static IServiceCollection AddFluentValidationConfig(this IServiceCollection Services)
	{
		Services
			.AddFluentValidationAutoValidation()
			.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

		return Services;
	}

	private static IServiceCollection AddAuthConfig(this IServiceCollection Services,
		IConfiguration configuration)
	{
		Services.AddIdentity<ApplicationUser, ApplicationRole>()
		  .AddEntityFrameworkStores<ApplicationDbContext>()
		  .AddDefaultTokenProviders();

		Services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
		Services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

		Services.AddSingleton<IJwtProvider, JwtProvider>();

		//Services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
		Services.AddOptions<JwtOptions>()
			.BindConfiguration(JwtOptions.SectionName)
			.ValidateDataAnnotations()
			;

		var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

		Services.AddAuthentication(options =>
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

		Services.Configure<IdentityOptions>(options =>
		{
			options.Password.RequiredLength = 8;
			 options.SignIn.RequireConfirmedEmail = true;
			options.User.RequireUniqueEmail = true;
		});
		return Services;
	}
	private static IServiceCollection AddBackgroundJobConfig(this IServiceCollection Services,
		IConfiguration configuration)
	{
		// Add Hangfire Services.
		Services.AddHangfire(config => config
			.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
			.UseSimpleAssemblyNameTypeSerializer()
			.UseRecommendedSerializerSettings()
			.UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

		// Add the processing server as IHostedServices
		Services.AddHangfireServer();

		return Services;
	}
}
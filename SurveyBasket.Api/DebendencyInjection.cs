﻿using FluentValidation.AspNetCore;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using SurveyBasket.Api.Authentication;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
		services.AddScoped<IPollService, PollService>();
		services.AddScoped<IQuestionService, QuestionService>();
		services.AddScoped<IVoteService, VoteService>();
		services.AddScoped<IResultService, ResultService>();

		services.AddScoped<ICacheService, CacheService>();

		services.AddExceptionHandler<GlobalExceptionHandler>();
		services.AddProblemDetails();
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
		  .AddEntityFrameworkStores<ApplicationDbContext>();


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

		return services;
	}
}
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Mnemo.Data;
using Mnemo.Data.Queries;
using Mnemo.Services.AccountService;
using Mnemo.Services.EnrichmentService;
using Mnemo.Services.EnrichmentService.ExternalDictionaries;
using Mnemo.Services.RepetitionService;
using Mnemo.Services.RepetitionService.Factories;
using Mnemo.Services.RepetitionService.Providers.DistractorProviders;
using Mnemo.Services.RepetitionService.Providers.TaskTypeProviders;
using Mnemo.Services.RepetitionService.Strategies;
using Mnemo.Services.VocabularyService;
using System.Text;

namespace Mnemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IConfigurationSection JwtSettings => Configuration.GetSection("Jwt");


        public void ConfigureServices(IServiceCollection services)
        {
            // Configurations
            services.Configure<EnrichmentOptions>(Configuration.GetSection("EnrichmentOptions"));
            services.Configure<RepetitionOptions>(Configuration.GetSection("RepetitionOptions"));
            services.Configure<SM2Options>(Configuration.GetSection("SM2Options"));

            // Add MemoryCache
            services.AddMemoryCache();

            // Add PostgreSQL context
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // Add Validators
            services.AddValidatorsFromAssemblyContaining<Program>();

            // Add AutoMapper
            services.AddAutoMapper(typeof(Program));

            // Add Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mnemo" });


                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });


                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            // JWT Authentication & Authorization services
            byte[] key = Encoding.UTF8.GetBytes(JwtSettings["Key"]);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtSettings["Issuer"],
                    ValidAudience = JwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            services.AddAuthorization();

            // DI Queries
            services.AddScoped<AccountQueries>();
            services.AddScoped<TaskQueries>();
            services.AddScoped<StateQueries>();
            services.AddScoped<VocabularyEntryQueries>();
            services.AddScoped<VocabularyQueries>();

            // DI Services
            services.AddScoped<AccountManagementService>();
            services.AddScoped<RepetitionTaskService>();
            services.AddScoped<StateManagementService>();
            services.AddScoped<QualityCalculationService>();
            services.AddScoped<EntryManagementService>();
            services.AddScoped<VocabularyManagementService>();

            // DI Enrichment
            services.AddHttpClient<IExternalDictionary, FreeDictionaryApi>();
            services.AddHostedService<EnrichmentBackgroundService>();

            // DI Distractor Providers
            services.AddScoped<AntonymDistractorProvider>();
            services.AddScoped<PrefixDistractorProvider>();
            services.AddScoped<ByPartOfSpeechDistractorProvider>();
            services.AddScoped<RandomDistractorProvider>();
            services.AddScoped<SyllableDistractorProvider>();
            services.AddScoped<IDistractorProvider, CompositeDistractorProvider>();

            // DI Task Factory
            services.AddScoped<ITaskTypeProvider, WeightTaskTypeProvider>();
            services.AddScoped<RepetitionTaskFactory>();

            // DI Task Strategies
            services.AddScoped<FastRepetitionTaskStrategy>();
            services.AddScoped<PlannedRepetitionTaskStrategy>();

            // Use Controllers
            services.AddControllers();
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            if (env.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }

        public void CheckConfiguration(IConfiguration config)
        {
            var messages = new List<string>();

            if (string.IsNullOrWhiteSpace(config.GetConnectionString("DefaultConnection")))
                messages.Add("ConnectionStrings:DefaultConnection is not configured");

            var jwt = config.GetSection("Jwt");
            var key = jwt["Key"];

            if (string.IsNullOrWhiteSpace(key))
                messages.Add("Jwt:Key is not configured");
            else if (Encoding.UTF8.GetByteCount(key) < 32)
                messages.Add($"Jwt:Key is too short ({Encoding.UTF8.GetByteCount(key)} bytes). Requires >= 32.");

            if (string.IsNullOrWhiteSpace(jwt["Issuer"]))
                messages.Add("Jwt:Issuer is not configured");

            if (string.IsNullOrWhiteSpace(jwt["Audience"]))
                messages.Add("Jwt:Audience is not configured");

            if (messages.Count > 0)
                throw new InvalidOperationException("Invalid configuration:\n" + string.Join("\n", messages.Select(e => " - " + e)));
        }
    }
}

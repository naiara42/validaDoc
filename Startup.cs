using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text.Encodings.Web;
using ValidadorCpfCnpjApi1.Middleware;

namespace ValidadorCpfCnpjApi1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Adiciona o serviço de CORS permitindo todas as origens e métodos
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Authorization header using the Bearer scheme (Example: '12345abcdef')",
                    Name = "ApiKey",
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
                        Array.Empty<string>()
                    }
                });

                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Validar documento.api", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            // Habilita o middleware de CORS com a política definida
            app.UseCors("AllowAllOrigins");

            // Middleware de validação da API Key
            app.UseMiddleware<ApiKeyMiddleware>(Configuration["ApiKey"]);

            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                Log.Information("Handling request: " + context.Request.Method + " " + context.Request.Path);
                await next.Invoke();
                Log.Information("Finished handling request.");
            });

            Log.Information("****************************************");
            Log.Information("*    Aplicação iniciada com sucesso!   *");
            Log.Information("****************************************");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
        {
            public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
                                               ILoggerFactory logger,
                                               UrlEncoder encoder,
                                               ISystemClock clock)
                : base(options, logger, encoder, clock)
            {
            }

            protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                if (!Request.Headers.ContainsKey("ApiKey"))
                {
                    return AuthenticateResult.Fail("API key is missing");
                }

                var apiKey = Request.Headers["ApiKey"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    return AuthenticateResult.Fail("API key is missing");
                }

                if (!IsApiKeyValid(apiKey))
                {
                    return AuthenticateResult.Fail("Invalid API key");
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "ApiKeyUser")
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }

            private bool IsApiKeyValid(string apiKey)
            {
                return apiKey == "Besouro";
            }
        }

        public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
        {
            public const string DefaultScheme = "ApiKeyScheme";
            public string Scheme => DefaultScheme;
            public string AuthenticationType = DefaultScheme;
        }
    }
}

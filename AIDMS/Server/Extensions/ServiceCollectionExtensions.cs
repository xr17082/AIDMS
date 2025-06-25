using AIDMS.Server.Services;
using AIDMS.Shared.Application.Configurations;
using AIDMS.Shared.Application.Interfaces.Serialization;
using AIDMS.Shared.Application.Interfaces.Services;
using AIDMS.Shared.Application.Serialization.Serializers;
using AIDMS.Shared.Application.Serialization.Settings;
using AIDMS.Shared.Constants.Application;
using AIDMS.Shared.Infrastructure;
using AIDMS.Shared.Infrastructure.Contexts;
using AIDMS.Shared.Infrastructure.Models.Identity;
using AIDMS.Shared.Wrapper;
using AIDMS.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using AIDMS.Shared.Application.Interfaces.Services.Identity;
using AIDMS.Shared.Infrastructure.Services.Identity;
using System.IO;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using AIDMS.Shared.Application.Interfaces.Services.Account.Identity;
using AIDMS.Shared.Infrastructure.Services;

namespace AIDMS.Server.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        internal static IServiceCollection ConfigureCosr(this IServiceCollection services, IConfiguration config)
        {
            //var appConfig = config.Get<AppConfiguration>();
            return services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowCredentials()
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           //Allow only specific ulr requlests
                           //.WithOrigins(appConfig.ApplicationUrl.TrimEnd('/'))
                           .AllowAnyOrigin();
                });
            });
        }

        internal static IServiceCollection AddCurrentUserService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            return services;
        }

        internal static IServiceCollection AddSerialization(this IServiceCollection services)
        {
            services.AddScoped<IJsonSerializerOptions, SystemTextJsonOptions>();
            services.AddScoped<IJsonSerializerSettings, NewtonsoftJsonSettings>();
            services.AddScoped<IJsonSerializer, SystemTextJsonSerializer>();
            return services;
        }

        internal static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            });
            services.AddTransient<IDataInitializer, DatabaseSeeder>();
            services.AddTransient<IDataInitializer, DocumentProcessor>();
            return services;
        }

        internal static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
            return services;
        }

        internal static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var appConfig = config.GetSection(nameof(AppConfiguration)).Get<AppConfiguration>();
            var key = Encoding.UTF8.GetBytes(appConfig.JwtSecret);

            services
                .AddAuthentication(auth =>
                {
                    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(async bearer =>
                {
                    bearer.RequireHttpsMetadata = false;
                    bearer.SaveToken = true;
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RoleClaimType = ClaimTypes.Role,
                        ClockSkew = TimeSpan.Zero
                    };

                    bearer.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments(ApplicationConstants.SignalR.HubUrl)))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentType = "application/json";
                                var result = JsonConvert.SerializeObject(Result.Fail("Expired Token"));
                                return context.Response.WriteAsync(result);
                            }
                            else
                            {
#if DEBUG                       
                                context.NoResult();
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.ContentType = "text/plain";
                                return context.Response.WriteAsync(context.Exception.ToString());
#else
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.ContentType = "application/json";
                                var result = JsonConvert.SerializeObject(Result.Fail("An unhandled error has occurred."));
                                return context.Response.WriteAsync(result);
#endif
                            }
                        },

                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            if (!context.Response.HasStarted)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentType = "application/json";
                                var result = JsonConvert.SerializeObject(Result.Fail("You are not authorized"));
                                return context.Response.WriteAsync(result);
                            }

                            return Task.CompletedTask;
                        },

                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(Result.Fail("You are not allowed to access this resource"));
                            return context.Response.WriteAsync(result);
                        }
                    };
                });
            services.AddAuthorization(options =>
            {
                foreach (var prop in typeof(Permissions).GetNestedTypes().SelectMany(c => c.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
                {
                    var propertyValue = prop.GetValue(null);
                    if (propertyValue is not null)
                    {
                        options.AddPolicy(propertyValue.ToString(), policy => policy.RequireClaim(ApplicationClaimTypes.Permission, propertyValue.ToString()));
                    }
                }
            });
            return services;
        }

        internal static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            //services.AddMediatR(Assembly.GetExecutingAssembly());
            return services;
        }

        internal static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MailConfiguration>(configuration.GetSection("MailConfiguration"));
            services.AddTransient<IMailService, SMTPMailService>();
            return services;
        }

        internal static void RegisterSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(async c =>
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!assembly.IsDynamic)
                    {
                        var xmlFile = $"{assembly.GetName().Name}.xml";
                        var xmlPath = Path.Combine(baseDirectory, xmlFile);
                        if (File.Exists(xmlPath))
                            c.IncludeXmlComments(xmlPath);
                    }
                }

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "AI DMS",
                    License = new OpenApiLicense
                    {
                        Name = "MIT Lisence",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - 'Bearer {token}' to access this API"
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
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });
        }

        internal static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<IRoleClaimService, RoleClaimService>();
            services.AddTransient<ITokenService, IdentityService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUploadService, UploadService>();
            services.AddTransient<IAuditService, AuditService>();
            services.AddScoped<IExcelService, ExcelService>();
            services.AddScoped<IOllamaService, OllamaService>();
            services.AddScoped<IDocumentProcessor, DocumentProcessor>();
            return services;
        }
    }
}

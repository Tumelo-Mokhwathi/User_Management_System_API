using User_Management_System_API.DataAccess;
using User_Management_System_API.DataAccess.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using User_Management_System_API.Configuration;
using User_Management_System_API.Services;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using User_Management_System_API.Services.Interface;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace User_Management_System_API
{
    public class Startup
    {
        private const string SwaggerName = "User Management API";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            InitialiseConfiguration(services);
            InitialiseDatabase(services);
            InitialiseIdentity(services);
            InitialiseAuthentication(services);
            InitialiseCors(services);
            InitialiseSwagger(services);
            InitialiseMail(services);
            InitialiseSecretKeys(services);
            InitialiseServices(services);
            InitialiseSMS(services);
        }

        private void InitialiseConfiguration(IServiceCollection services)
        {
            services.Configure<OidcOptions>(Configuration.GetSection("Oidc"));
        }

        private void InitialiseAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(option =>
            {
                option.SaveToken = true;
                option.RequireHttpsMetadata = false;
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["Oidc:Audience"],
                    ValidIssuer = Configuration["Oidc:Authority"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Oidc:ClientSecret"].ToString())),
                };
            });
        }

        private static void InitialiseSwagger(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var projectVersion = AssemblyName.GetAssemblyName(assembly.Location).Version.ToString();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                "v1",
                new OpenApiInfo { Title = SwaggerName, Version = projectVersion });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme.",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

        }

        private void InitialiseDatabase(IServiceCollection services)
        {
            var migrationsAssembly = typeof(ApplicationDbContext).GetTypeInfo().Assembly.GetName().Name;
            services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(
                Configuration.GetConnectionString("ApplicationDb"),
                b => b.MigrationsAssembly(migrationsAssembly)));
        }

        private void InitialiseIdentity(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.SignIn.RequireConfirmedEmail = true;
                config.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<EmailConfirmationTokenProvider<ApplicationUser>>("emailconfirmation");
        }

        private void InitialiseMail(IServiceCollection services)
        {
            services.AddTransient<IEmailSender, MessagingService>();
            services.Configure<MailOptions>(Configuration.GetSection("Mail"));
            services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
               opt.TokenLifespan = TimeSpan.FromDays(3));
        }

        private void InitialiseSMS(IServiceCollection services)
        {
            services.AddTransient<ISmsSender, MessagingService>();
            services.Configure<SMSoptions>(Configuration.GetSection("SMS"));
        }

        private void InitialiseServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
        }

        private void InitialiseSecretKeys(IServiceCollection services)
        {
            services.Configure<Credentials>(Configuration.GetSection("SecretKeys"));
        }

        private void InitialiseCors(IServiceCollection services)
        {
            var corsOpts = Configuration.GetSection("Cors").Get<CorsOptions>();

            services.AddCors(option =>
            {
                option.AddPolicy(Constants.General.AllowSpecificOriginsName,
                    builder => builder.AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .WithOrigins(corsOpts.GetAllowedOriginsAsArray()));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors(Constants.General.AllowSpecificOriginsName);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", SwaggerName));
        }
    }
}

﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Text;
using TNDStudios.Web.ApiManager;
using TNDStudios.Web.ApiManager.Data.Soap;
using TNDStudios.Web.ApiManager.Security.Authentication;

namespace Website
{
    public class Startup
    {
        // Configuration Items available to the system from app settings
        public static String JWTKey { get; internal set; } = String.Empty;
        public static String JWTIssuer { get; internal set; } = String.Empty;
        public static String JWTAudience { get; internal set; } = String.Empty;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            IConfigurationSection securityKeys = configuration.GetSection("SecurityKeys");
            if (securityKeys != null)
            {
                JWTKey = securityKeys.GetValue<String>("JWTKey");
                JWTIssuer = securityKeys.GetValue<String>("JWTIssuer");
                JWTAudience = securityKeys.GetValue<String>("JWTAudience");
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Set up the authentication service with the appropriate authenticator implementation
            FileStream accessControl = File.OpenRead(Path.Combine(Environment.CurrentDirectory, "users.json"));
            IUserAuthenticator userAuthenticator = new UserAuthenticator(
                                                        new TokenValidationParameters()
                                                        {
                                                            ValidateLifetime = true,
                                                            ValidateAudience = true,
                                                            ValidateIssuer = true,
                                                            ValidIssuer = JWTIssuer,
                                                            ValidAudience = JWTAudience,
                                                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTKey))
                                                        });
            userAuthenticator.RefreshAccessList(accessControl);

            // If a session is ever needed
            /*
            services
                .AddDistributedMemoryCache()
                .AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(60);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });
            */

            // Regular system setup
            services
                .AddCors()
                .AddLogging()
                .AddMvc(options =>
                    {
                        options.InputFormatters.Add(new SoapFormatter());
                    })
                /*
                .AddJsonOptions(
                    options => 
                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                )*/
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Custom service setup for the API Manager
            services
                .AddCustomAuthentication(userAuthenticator)
                .AddCustomVersioning();

            // Add authorisation by policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Admin"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            app.UseAuthentication();

            app.UseHttpsRedirection();
            //app.UseSession();
            app.UseMvc();

            // Custom app builder setup for the API Manager
            app.UseCustomVersioning(provider);
        }
    }
}

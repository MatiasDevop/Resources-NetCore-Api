using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetbook.Authorization;
using Tweetbook.Options;
using Tweetbook.Services;
using Tweetbook.Filters;

namespace Tweetbook.Installers
{
    public class MvcInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // set up jwt
            var jwtSettings = new JwtSettings();
            configuration.Bind(nameof(jwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);

            //DI
            services.AddScoped<IIdentityService, IdentityService>();
            // from Startup
            services     
                .AddMvc( options => 
                {
                    options.Filters.Add<ValidationFilter>();
                })
                .AddFluentValidation(mvcConfigguration => mvcConfigguration.RegisterValidatorsFromAssemblyContaining<Startup>()) //add this for our validations
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            //start set up jwt
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = false,
                ValidateLifetime = true
            };

            services.AddSingleton(tokenValidationParameters);

            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.SaveToken = true;
                    x.TokenValidationParameters = tokenValidationParameters;
                });//end set up jwt

            //Add Polices for Account and Domain especific
            services.AddAuthorization(options => 
            {
                options.AddPolicy("MustWorkForChapsas", policy =>
                {
                    // you can also add new requirement as roles and soonpolicy.RequireRole
                    policy.AddRequirements(new WorksForCompanyRequirement("chapsas.com"));
                });
            });

            services.AddSingleton<IAuthorizationHandler, WorksForCompanyHandler>();
            // en policy by email.com domain

            //authorizaition on claims only if the user has this Claim he can actually access
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("TagViewer", builder => 
            //    {   //here you can add more Claoims
            //        builder.RequireClaim("tags.view", "true");
            //    });
            //});

           
        }
    }
}

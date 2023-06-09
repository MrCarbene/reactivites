using System.Text;
using API.Services;
using Domain;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            services
                .AddIdentityCore<AppUser>(options =>
                {
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<DataContext>()
                .AddSignInManager<SignInManager<AppUser>>();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateAudience = false,
                        ValidateIssuer = false
                    };
                });

            // Configre custom policy with Identity
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy(
                    "IsActivityHost",
                    policy =>
                    {
                        policy.Requirements.Add(new IsHostRequirement());
                    }
                );
            });
            services.AddTransient<IAuthorizationHandler, IsHostRequirementHandler>();
            // scoped to the lifetime of the request
            services.AddScoped<TokenService>();

            return services;
        }
    }
}

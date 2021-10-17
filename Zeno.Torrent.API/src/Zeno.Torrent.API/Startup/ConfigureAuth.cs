using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.API.Util;
using Zeno.Torrent.Service.API.Auth;

namespace Zeno.Torrent.API {

    public static class ConfigureAuth {

        public static void ConfigureAuthServices(
            this IServiceCollection services,
            AppSettings appSettings
        ) {
            services
                .AddAuthorization(o => {
                    o.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireSubClaim(AuthConstants.Token.Scope, AuthConstants.API.Scope)
                        .Build();
                })
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o => {
                    o.SaveToken = true;
                    o.Authority = appSettings.AUTHORITY;
                    o.TokenValidationParameters = new TokenValidationParameters {
                        ClockSkew = new TimeSpan(0, 0, 30),
                        ValidateAudience = false,
                    };
                });
        }

    }

}

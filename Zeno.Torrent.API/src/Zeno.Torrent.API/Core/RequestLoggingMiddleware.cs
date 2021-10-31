using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Zeno.Torrent.Service.API.Auth;

namespace Zeno.Torrent.API.Core {
    public class AddRolesClaimsTransformation : IClaimsTransformation {
        private readonly IHttpContextAccessor httpContextAccessor;
        public AddRolesClaimsTransformation(IHttpContextAccessor httpContextAccessor) {
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal) {
            if (httpContextAccessor.HttpContext != null) {
                var clone = principal.Clone();
                var newIdentity = (ClaimsIdentity)clone.Identity;
                if (httpContextAccessor.HttpContext.Items.TryGetValue(AuthConstants.Token.ProxiedUserId, out var v)) {
                    var claim = new Claim(AuthConstants.Token.ProxiedUserId, v as string);
                    newIdentity.AddClaim(claim);
                }

                return await Task.FromResult(clone);
            }

            return principal;
        }
    }

    public class RequestResponseLoggingMiddleware {
        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext context) {
            if (!context.Request.Path.ToString().Contains("health")) {
                if (!(context.Request.Method == "GET" && context.Request.Path == "/api/download") &&
                    !(context.Request.Method == "GET" && context.Request.Path == "/api/download/all/state")) {
                    Console.WriteLine("========== REQ START ==========");
                    Console.WriteLine("REQUEST: {0} {1}", context.Request.Method, context.Request.Path);
                }
                await _next.Invoke(context);
                if (!(context.Request.Method == "GET" && context.Request.Path == "/api/download") &&
                    !(context.Request.Method == "GET" && context.Request.Path == "/api/download/all/state")) {
                    Console.WriteLine("RESPONSE: {0}", context.Response.StatusCode);
                    Console.WriteLine("========== REQ END ==========");
                }
            }
        }

    }
}

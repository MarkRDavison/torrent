using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Zeno.Torrent.Service.API.Auth;

namespace Zeno.Torrent.API.Core {

    public class ExtractSubHeaderMiddleware {

        private readonly RequestDelegate _next;

        public ExtractSubHeaderMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext context) {
            if (context.User.Identity.IsAuthenticated) {
                bool valid = false;
                if (context.Request.Headers.TryGetValue(AuthConstants.Token.Sub, out var v)) {
                    if (v.Count == 1) {
                        context.Items.Add(AuthConstants.Token.ProxiedUserId, v[0]);
                        valid = true;
                    }
                }

                if (!valid) {
                    context.Response.StatusCode = 401; //UnAuthorized
                    context.Response.Headers.Add("WWW-Authenticate", "Invalid Sub Header");
                    return;
                }
            }

            await _next.Invoke(context);
        }

    }

}

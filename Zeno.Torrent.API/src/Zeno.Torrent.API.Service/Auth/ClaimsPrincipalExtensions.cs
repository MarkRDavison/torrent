using System;
using System.Linq;
using System.Security.Claims;
using Zeno.Torrent.Service.API.Auth;

namespace Zeno.Torrent.API.Service.Auth {
    public static class ClaimsPrincipalExtensions {
        public static string SubjectId(this ClaimsPrincipal user) {
            return user?.Claims?
                .FirstOrDefault(c =>
                    c.Type.Equals(ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase) ||
                    c.Type.Equals(AuthConstants.Token.Sub, StringComparison.OrdinalIgnoreCase))?.Value;
        }
        public static string ProxiedUserSubjectId(this ClaimsPrincipal user) {
            return user?.Claims?
                .FirstOrDefault(c =>
                    c.Type.Equals(AuthConstants.Token.ProxiedUserId, StringComparison.OrdinalIgnoreCase))?.Value
                ?? user.SubjectId();
        }
    }
}

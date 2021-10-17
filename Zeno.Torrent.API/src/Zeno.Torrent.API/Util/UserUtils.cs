using System.Security.Claims;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Service.Auth;

namespace Zeno.Torrent.API.Util {

    public class UserUtils {

        public static User Create(ClaimsPrincipal user) {
            return new User {
                Sub = user.ProxiedUserSubjectId()
            };
        }

    }

}

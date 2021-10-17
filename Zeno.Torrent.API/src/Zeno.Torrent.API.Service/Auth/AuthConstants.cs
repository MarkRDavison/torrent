namespace Zeno.Torrent.Service.API.Auth {

    public static class AuthConstants {

        public static class Token {
            public static string Azp = "azp";
            public static string Scope = "scope";
            public static string Sub = "sub";
            public static string ProxiedUserId = "ProxiedUserId";
        }

        public static class Policy {
            public static string ZenoScope = "ZenoScopePolicy";
            public static string ZenoAuthorizedParty = "ZenoAuthorizedPartyPolicy";
        }

        public static class API {
            public static string Scope = "zeno";
        }

    }

}

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Api
{
    public static class SD
    {
        public const string Facebook = "facebook";
        public const string Google = "google";

        //roles
        public const string AdminRole = "Admin";
        public const string ManagerRole = "Manager";
        public const string PlayerRole = "Player";

        public const string AdminUserName = "admin@example.com";
        public const string SuperAdminChangeNotAllowed = "Super Admin chagne is not allowed";
        public const int MaximumLoginAttemp = 3;
        public static bool VIPPolicy(AuthorizationHandlerContext context)
        {
            if(context.User.IsInRole(PlayerRole) &&
                context.User.HasClaim(c => c.Type == ClaimTypes.Email && c.Value.Contains("vip")))
            {
                return true;
            }

            return false;
        }
    }
}

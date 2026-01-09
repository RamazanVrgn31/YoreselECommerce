using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Core.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static List<string> Claims(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            var result = claimsPrincipal?.FindAll(claimType)?.Select(x => x.Value).ToList();
            return result;
        }

        public static List<string> ClaimRoles(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal?.Claims(ClaimTypes.Role);
        }

        public static int? GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null) return null;

            var claims = claimsPrincipal.Claims.ToList();

            // 1. Senin Token'ındaki uzun SOAP adresi (Öncelikli)
            var claim = claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

            // 2. Standart .NET sabiti
            if (claim == null)
            {
                claim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            }

            // 3. Kısa "nameid"
            if (claim == null)
            {
                claim = claims.FirstOrDefault(x => x.Type == "nameid");
            }

            // 4. "sub" (Subject)
            if (claim == null)
            {
                claim = claims.FirstOrDefault(x => x.Type == "sub");
            }

            // Sonuç varsa int'e çevirip döndür
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }

    }
}
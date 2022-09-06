namespace EmbedIO.Samples
{
    using BearerToken;
    using Swan.Logging;
    using System;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Utilities;

    internal class SampleAuthorizationServerProvider : IAuthorizationServerProvider
    {
        public async Task ValidateClientAuthentication(ValidateClientAuthenticationContext context)
        {

            var data = await context.HttpContext.GetRequestFormDataAsync().ConfigureAwait(false);

            if (data!=null && data.ContainsKey("mail") && data.ContainsKey("password"))
            {
                context.Identity.AddClaim(new System.Security.Claims.Claim("Role", "Admin"));
                context.Validated(data.ContainsKey("username") ? data["username"] : string.Empty);
            }
            else
            {
                context.Rejected();
            }
        }

        /*
        private async Task<string> ValidateAndGetUsername(ValidateClientAuthenticationContext context)
        {
            var data = await context.HttpContext.GetRequestFormDataAsync().ConfigureAwait(false);
            if (data.Get("grant_type") != "password")
                return null;

            var username = data.Get("username");
            var password = data.Get("password");
            var user = BusinessLayer.CheckUserAndPassword(
                username ?? string.Empty,
                password ?? string.Empty);

            context.HttpContext.Items[UserKey] = user;
            return user != null ? username : null;
        }
        */

        public long GetExpirationDate() => DateTime.UtcNow.AddHours(12).Ticks;
    }

    internal static class HttpContextExtensions
    {
        public static User GetUser(this IHttpContext @this)
        {
            string bearerToken = @this.Request.Headers["Authorization"];
            if (bearerToken == null)
            {
                return null;
            }
            //bearerToken = bearerToken.Substring(7, bearerToken.Length-7);
            User user = Database.Instance.GetUsers().FirstOrDefault(u => u.Token == bearerToken);
            return user;
        }
    }
}
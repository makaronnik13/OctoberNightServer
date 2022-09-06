using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Swan;
using Swan.Cryptography;

namespace EmbedIO.Samples
{
    public class User
    {
        [LiteDB.BsonId]
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public LocationData Location { get; set; }
        /*
#pragma warning disable 0618 // "Use a better hasher." - Not our fault if gravatar.com uses MD5.
        public string PhotoUrl => $"http://www.gravatar.com/avatar/{Hasher.ComputeMD5(EmailAddress).ToUpperHex()}.png?s=100";
#pragma warning restore 0618
        */

        internal static async Task<IEnumerable<User>> GetDataAsync()
        {
            return Database.Instance.GetUsers();
        }
    }
}
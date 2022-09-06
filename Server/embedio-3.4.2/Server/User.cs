using System.Collections.Generic;
using System.Numerics;
using System.Security.Principal;
using System.Threading.Tasks;
using LiteDB;
using Swan;
using Swan.Cryptography;

namespace EmbedIO.Samples
{
    public class User
    {
        [BsonId]
        public string EmailAddress { get; set; }

        public string Name { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public Position2d Position { get; set; }

        public static async Task<IEnumerable<User>> GetDataAsync()
        {
            return Database.Instance.GetUsers();
        }
    }
}
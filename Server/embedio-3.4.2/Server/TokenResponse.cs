using System.Threading.Tasks;

namespace EmbedIO.Samples
{
    public class TokenResponse
    {
        public string Token { get; set; }

        public TokenResponse(string token)
        {
            Token = token;
        }
    }
}
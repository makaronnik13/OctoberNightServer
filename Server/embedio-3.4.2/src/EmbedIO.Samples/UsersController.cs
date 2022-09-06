using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using Swan.Logging;
using Unosquare.Tubular;

namespace EmbedIO.Samples
{
    // A very simple controller to handle People CRUD.
    // Notice how it Inherits from WebApiController and the methods have WebApiHandler attributes 
    // This is for sampling purposes only.
    public sealed class UsersController : WebApiController
    {
        // Gets all records.
        // This will respond to 
        //     GET http://localhost:9696/api/people
        [Route(HttpVerbs.Get, "/users")]
        public Task<IEnumerable<User>> GetAllUsers() => Samples.User.GetDataAsync();


        // Posts the people Tubular model.
        [Route(HttpVerbs.Post, "/register")]
        public async Task<TokenResponse> Register()
        {
            var res = await HttpContext.GetRequestDataAsync<User>().ConfigureAwait(false);
            User user = (await Samples.User.GetDataAsync().ConfigureAwait(false)).FirstOrDefault(x => x.Name == res.Name);
            User user2 = (await Samples.User.GetDataAsync().ConfigureAwait(false)).FirstOrDefault(x => x.EmailAddress == res.EmailAddress);
            if (user!=null)
            {
                throw HttpException.BadRequest("nickname already taken");
            }
            if (user2 != null)
            {
                throw HttpException.BadRequest("email already taken");
            }
            res.Token = System.Guid.NewGuid().ToString();
            Database.Instance.AddUser(res);
            return new TokenResponse(res.Token);
        }

        // Posts the people Tubular model.
        [Route(HttpVerbs.Post, "/login")]
        public async Task<TokenResponse> Login()
        {
            var res = await HttpContext.GetRequestDataAsync<User>().ConfigureAwait(false);
            User user = (await Samples.User.GetDataAsync().ConfigureAwait(false)).FirstOrDefault(x => x.Name == res.Name);

            if (user == null)
            {
                throw HttpException.BadRequest("user not found");
            }
            if (user.Password!=res.Password)
            {
                throw HttpException.BadRequest("wrong password");
            }

            return new TokenResponse(user.Token);
        }

        // Posts the people Tubular model.
        [Route(HttpVerbs.Post, "/setLocation")]
        public async Task SetLocation()
        {
            LocationData location = await HttpContext.GetRequestDataAsync<LocationData>().ConfigureAwait(false);
           
        }


        // Select by name
        [Route(HttpVerbs.Get, "/userByName/{name}")]
        public async Task<User> GetUserByName(string name)
        {
            User user = (await Samples.User.GetDataAsync().ConfigureAwait(false)).FirstOrDefault(x => x.Name == name);
            
            if (user == null)
            {
                throw HttpException.NotFound();
            }

            user.Password = string.Empty;
            user.Token = string.Empty;

            return user;
        }
    }
}
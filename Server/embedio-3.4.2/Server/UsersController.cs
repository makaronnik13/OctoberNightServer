using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using HttpMultipartParser;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Server;
using Swan.Logging;

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

        [Route(HttpVerbs.Get, "/user")]
        public string GetUserName() =>
           HttpContext.User.Identity.Name;



        // Posts the people Tubular model.
        [Route(HttpVerbs.Post, "/register")]
        public async Task<TokenResponse> Register()
        {
            var res = await HttpContext.GetRequestDataAsync<User>().ConfigureAwait(false);
            User user = (await Samples.User.GetDataAsync().ConfigureAwait(false)).FirstOrDefault(x => x.Name == res.Name);
            User user2 = (await Samples.User.GetDataAsync().ConfigureAwait(false)).FirstOrDefault(x => x.EmailAddress == res.EmailAddress);

            if (user2 != null)
            {
                throw HttpException.BadRequest("email already taken");
            }

            if (user!=null)
            {
                throw HttpException.BadRequest("nickname already taken");
            }
            
            res.Token = Guid.NewGuid().ToString();
            Database.Instance.AddUser(res);
            return new TokenResponse(res.Token);
        }

        // Posts the people Tubular model.
        [Route(HttpVerbs.Post, "/login")]
        public async Task<TokenResponse> Login()
        {
            var res = await HttpContext.GetRequestDataAsync<User>().ConfigureAwait(false);
            User user = (await Samples.User.GetDataAsync().ConfigureAwait(false)).FirstOrDefault(x => x.EmailAddress == res.EmailAddress);

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
        [Route(HttpVerbs.Post, "/upd_position")]
        public async Task SetUserPosition()
        {
            User user = HttpContext.GetUser();

            if (user!=null)
            {
                Position2d position = await HttpContext.GetRequestDataAsync<Position2d>().ConfigureAwait(false);

                user.Position = position;

                Database.Instance.UpdateUser(user);
            }
            else
            {
                throw HttpException.Unauthorized();
            }
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

        private static float visibleLocationsRadius = 3;  
        
        [Route(HttpVerbs.Get, "/get_locations")]
        public async Task<List<LocationModel>> Getpoints()
        {
            User user = HttpContext.GetUser();

            if (user != null)
            {
                List<LocationModel> models = Database.Instance.GetPoints().Where(p=>Position2d.Distance(p.Position, user.Position)<=visibleLocationsRadius).ToList();
                return models;
            }
            else
            {
                throw HttpException.Unauthorized();
            }
        }

        [Route(HttpVerbs.Post, "/upd_location")]
        public async Task UpdatePoint()
        {
            User user = HttpContext.GetUser();

            if (user != null)
            {
                LocationModel point = await HttpContext.GetRequestDataAsync<LocationModel>().ConfigureAwait(false);
                
                
                Database.Instance.AddOrUpdatePoint(point);
            }
            else
            {
                throw HttpException.Unauthorized();
            }
        }

        
        [Route(HttpVerbs.Post, "/upd_event")]
        public async Task UpdateEvent()
        {
            User user = HttpContext.GetUser();

            if (user != null)
            {
                EventModel eventModel = await HttpContext.GetRequestDataAsync<EventModel>().ConfigureAwait(false);
            }
            else
            {
                throw HttpException.Unauthorized();
            }
        }
        

        [Route(HttpVerbs.Post, "/upd_texture")]
        public async Task UpdateTexture()
        {
            User user = HttpContext.GetUser();

            if (user != null)
            {
                if (Request.ContentType == null)
                {
                    throw new ArgumentNullException("Content type is null! Can't get boundary.");
                }

                string boundaryTag = "boundary=";
                string boundary = "";
                int ix = Request.ContentType.IndexOf(boundaryTag);
                if (ix != -1)
                {
                    boundary = Request.ContentType.Substring(ix + boundaryTag.Length);
                    // do something here
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Boundary tag not found!");
                }


                var parser = await MultipartFormDataParser.ParseAsync(Request.InputStream, boundary: boundary, encoding: Request.ContentEncoding, binaryBufferSize: (int)Request.ContentLength64, ignoreInvalidParts: true) ;
                
                FilePart file = parser.Files.First();

                Database.Instance.AddOrUpdateTexture(file); 
            }
            else
            {
                throw HttpException.Unauthorized();
            }
        }

      
        
        [Route(HttpVerbs.Get, route: "/get_texture")]
        public async Task GetTexture()
        {
            User user = HttpContext.GetUser();

            //texture.Info();
            "getTexture".Info();
            
            if (user != null)
            {
                string? textureName = HttpContext.Request.Headers["TextureName"];
                //TextureRequest textureRequest = await HttpContext.GetRequestDataAsync<TextureRequest>().ConfigureAwait(false);

                
                ($@"{textureName}.png").Info();
                
                using (var stream = HttpContext.OpenResponseStream())
                {
                    using (var readStream = Database.Instance.GetTexture( $@"{textureName}.png"))
                    {
                        if (readStream!=null)
                        {
                            byte[] buffer = new byte[readStream.Length];
                            readStream.Read(buffer);
                            stream.Write(buffer);
                        }
                        else
                        {
                            throw HttpException.NotFound();
                        }
                    }
                 
                }
            }
        }
    }
}
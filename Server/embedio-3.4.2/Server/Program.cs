using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.Security;
using EmbedIO.WebApi;
using EmbedIO.BearerToken;
using Swan;
using Swan.Logging;
using Microsoft.IdentityModel.Logging;
using System.Net;
using Swan.Formatters;
using EmbedIO.Testing;

namespace EmbedIO.Samples
{
    internal class Program
    {
        private const bool OpenBrowser = false;
        private const bool UseFileCache = true;
        private static Database database;
        private static string url = "http://*:9696/";

        private static void Main(string[] args)
        {
            database = new Database();

            using (var ctSource = new CancellationTokenSource())
            {
                Task.WaitAll(
                    RunWebServerAsync(url, ctSource.Token),
                    WaitForUserBreakAsync(ctSource.Cancel));
            }
            // Clean up
            "Bye".Info(nameof(Program));
            Terminal.Flush();
            Console.WriteLine("Press any key to exit.");
            WaitForKeypress();
        }

        // Create and configure our web server.
        private static WebServer CreateWebServer(string url)
        {
            WebServer server = new WebServer(url);
            server
                //.WithBearerToken("/", "0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF", new SampleAuthorizationServerProvider())
                .WithWebApi("/api", m =>
                {
                    m.RegisterController<UsersController>();
                });

            /*
            var authServer = new BasicAuthorizationServerProvider();
            var bearerTokenModule = new BearerTokenModule(
               "/api",
               authServer,
               "0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF")
            {
                OnSuccessTransformation = dict => { dict.Add("logged", true); },
            };

           
#pragma warning disable CA2000 // Call Dispose on object - this is a factory method.
            var server = new WebServer(o => o
            .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                //.WithBearerToken(url, "0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9eyJjbGF", authServer)
                .WithModule(bearerTokenModule)
                .WithIPBanning(o => o
                    .WithMaxRequestsPerSecond()
                    .WithRegexRules("HTTP exception 404"))
                .WithLocalSessionManager()
                .WithCors(
                    // Origins, separated by comma without last slash
                    "http://unosquare.github.io,http://run.plnkr.co",
                    // Allowed headers
                    "content-type, accept",
                    // Allowed methods
                    "post")
                .WithWebApi("/api", m => m
                    .WithController<UsersController>())
                //.WithModule(new WebSocketChatModule("/chat"))
                //.WithModule(new WebSocketTerminalModule("/terminal"))
                .WithStaticFolder("/", HtmlRootPath, true, m => m
                    .WithContentCaching(UseFileCache)) // Add static files after other modules to avoid conflicts
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));
            */

            // Listen for state changes.
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
#pragma warning restore CA2000
        }

        // Create and run a web server.
        private static async Task RunWebServerAsync(string url, CancellationToken cancellationToken)
        {
            using var server = CreateWebServer(url);

            await server.RunAsync(cancellationToken).ConfigureAwait(false);
        }

        // Open the default browser on the web server's home page.
#pragma warning disable CA1801 // Unused parameter
        private static async Task ShowBrowserAsync(string url, CancellationToken cancellationToken)
#pragma warning restore CA1801
        {
            // Be sure to run in parallel.
            await Task.Yield();

            // Fire up the browser to show the content!
            using var browser = new Process();
            browser.StartInfo = new ProcessStartInfo(url)
            {
                UseShellExecute = true
            };
            browser.Start();
        }

        // Prompt the user to press any key; when a key is next pressed,
        // call the specified action to cancel operations.
        private static async Task WaitForUserBreakAsync(Action cancel)
        {
            // Be sure to run in parallel.
            await Task.Yield();

 
            //await GetValidToken();

            "Press any key to stop the web server.".Info(nameof(Program));
            WaitForKeypress();
            "Stopping...".Info(nameof(Program));
            cancel();
        }

        /*
        public static void GetInvalidToken()
        {
            var payload = System.Text.Encoding.UTF8.GetBytes("grant_type=nothing");
            var request = (HttpWebRequest)WebRequest.Create(url + "token");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = payload.Length;

            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(payload, 0, payload.Length);
            }

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                var response = (HttpWebResponse)ex.Response;

                response.StatusCode.ToString().Info();

            }
        }
        */

        public static async Task GetValidToken()
        {
            TestHttpClient Client = TestHttpClient.Create(url);

            var username = "test";
            var payload = System.Text.Encoding.UTF8.GetBytes($"grant_type=password&username={username}&password={username}");

            using var req = new HttpRequestMessage(HttpMethod.Post, url + "token") { Content = new ByteArrayContent(payload) };

            using var res = await Client.SendAsync(req);
            var jsonString = await res.Content.ReadAsStringAsync();
            var json = Json.Deserialize<BearerToken.BearerToken>(jsonString);
            var token = json.Token;

            /*
            var indexRequest = new HttpRequestMessage(HttpMethod.Get, url + "api/user");

            using (var userResponse = await Client.SendAsync(indexRequest))
            {
                //Assert.AreEqual(userResponse.StatusCode, HttpStatusCode.Unauthorized);
            }

            indexRequest = new HttpRequestMessage(HttpMethod.Get, url + "api/user");
            indexRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using (var userResponse = await Client.SendAsync(indexRequest))
            {
                //Assert.AreEqual(userResponse.StatusCode, HttpStatusCode.OK);
                var userResult = await userResponse.Content.ReadAsStringAsync();
                //Assert.AreEqual(userResult, username);
            }*/
        }

        // Clear the console input buffer and wait for a keypress
        private static void WaitForKeypress()
        {
            while (Console.KeyAvailable)
                Console.ReadKey(true);

            Console.ReadKey(true);
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace HttpListenerExample
{
    // Class that serves up an HTTP server and allows for shutting down of the machine
    // Based on: https://gist.github.com/define-private-public/d05bc52dd0bed1c4699d49e2737e80e7
    class ShutdownServer
    {
        public static HttpListener listener;
        public static async Task HandleIncomingConnections()
        {
            while (true)
            {
                HttpListenerContext ctx = await listener.GetContextAsync(); // Wait for connection
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                if (req == null || resp == null) continue;

                // Print request info
                Console.WriteLine();
                Console.WriteLine("URL        : " + req.Url.ToString());
                Console.WriteLine("Method     : " + req.HttpMethod);
                Console.WriteLine("Hostname   : " + req.UserHostName);
                Console.WriteLine("User agent : " + req.UserAgent);

                // Write the response info
                string pageData =
                   "<!DOCTYPE html>" +
                   "<html>" +
                   "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" +
                   "  <head><title>Shutdown Tool</title></head>" +
                   "  <body style='background-color:Black;color:SeaShell;'>" +
                   "    <p>{0}</p>" +
                   "    <p>{1}</p>" +
                   "    <p><form method='post' action='shutdown'><input type='submit' value='Shutdown NOW'></form></p>" +
                   "    <p><form method='post' action='shutdown1'><input type='submit' value='Shutdown 1 min'></form></p>" +
                   "    <p><form method='post' action='shutdown5'><input type='submit' value='Shutdown 5 min'></form></p>" +
                   "    <p><form method='post' action='shutdown10'><input type='submit' value='Shutdown 10 min'></form></p>" +
                   "    <p><form method='post' action='restart'><input type='submit' value='Restart NOW'></form></p>" +
                   "    <p><form method='post' action='cancel'><input type='submit' value='Cancel Shutdown'></form></p>" +
                   "  </body>" +
                   "</html>";

                byte[] data = Encoding.UTF8.GetBytes(String.Format(pageData, System.Net.Dns.GetHostName(), GetUpTime()));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();

                if (req.HttpMethod == "POST")
                {
                    switch(req.Url.AbsolutePath)
                    {
                        case "/shutdown":
                            Console.WriteLine("Action     : Request shutdown NOW");
                            Process.Start("shutdown", String.Format("/s /t 1"));
                            break;
                        case "/shutdown1":
                            Console.WriteLine("Action     : Request shutdown in 1 minute");
                            Process.Start("shutdown", String.Format("/s /t 60"));
                            break;
                        case "/shutdown5":
                            Console.WriteLine("Action     : Request shutdown in 5 minutes");
                            Process.Start("shutdown", String.Format("/s /t 300"));
                            break;
                        case "/shutdown10":
                            Console.WriteLine("Action     : Request shutdown in 10 minutes");
                            Process.Start("shutdown", String.Format("/s /t 600"));
                            break;
                        case "/restart":
                            Console.WriteLine("Action     : Request restart NOW");
                            Process.Start("shutdown", String.Format("/r /t 1"));
                            break;
                        case "/cancel":
                            Console.WriteLine("Action     : Cancel shutdown");
                            Process.Start("shutdown", "/a");
                            break;
                    }
                }
            }
        }

        public static void Start(string url)
        {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }

        // https://stackoverflow.com/questions/972105/retrieve-system-uptime-using-c-sharp

        public static TimeSpan GetUpTime()
        {
            return TimeSpan.FromMilliseconds(GetTickCount64());
        }

        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();
    }
}

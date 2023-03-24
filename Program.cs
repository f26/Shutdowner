using HttpListenerExample;

namespace Shutdowner
{
    // TODO:
    // Place at some location on computer
    // Disable firewall (or port at least)
    // Download NSSM: http://nssm.cc/download
    // Set up as a service: nssm install Shutdowner
    //    This opens a GUI, fill it out!
    // Start the service: nssm start Shutdowner

    internal class Program
    {
        static void Main(string[] args)
        {
            ShutdownServer.Start("http://*:5320/");
        }
    }

}
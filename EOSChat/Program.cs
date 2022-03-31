using System;
using System.Threading;

using EOSChat.Application;

namespace EOSChat
{
    class Program
    {
        static void Main(string[] args)
        {
            Config.ConfigurationReader.SetAutomaticConfigurations();

            FileCredentials.CredentialWriter.SetupCredentialStaticFiles();

            Console.WriteLine("[EOS] Starting Program at " + DateTime.Now.ToString());
            BackendService.StartService("0.0.0.0", 52030);
            DeliveryService.StartService();

            Thread _resourceThread = new Thread(() => ResourceMeters.StartResourcing());
            _resourceThread.Start();

            while (true)
            {
                Config.ConfigurationReader.SetAutomaticConfigurations(); // reads configuration every 1 second, dynamic updating is safe!
                Thread.Sleep(10000);
            }
        }
    }
}

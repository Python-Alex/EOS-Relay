using System;
using System.Threading;

using EOSChat.Application;

namespace EOSChat
{
    class Program
    {
        static void Main(string[] args)
        {
            FileCredentials.CredentialWriter.SetupCredentialStaticFiles();

            Console.WriteLine("[EOS] Starting Program at " + DateTime.Now.ToString());
            BackendService.StartService("0.0.0.0", 52030);
            DeliveryService.StartService();

            while (true)
            {
                Thread.Sleep(500);
            }
        }
    }
}

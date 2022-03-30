using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EOSChat
{
    public static class ThreadPersistance
    {
        // sloppy implementation of mutexing a global variable.
        // this will later be changed to an actual thread mutex
        public static bool RemovingClientCallWaiting = false;
    }

    public static class BackendService
    {
        public static Socket listenerSocket;
        public static IPEndPoint ipEndpoint;

        public static void StartService(string address, int port)
        {
            ipEndpoint = new IPEndPoint(IPAddress.Parse(address), port);
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listenerSocket.Bind(ipEndpoint);
                listenerSocket.Listen(0);
            }
            catch (Exception error)
            {
                Console.WriteLine(String.Format("[SocketListener] ERROR - {0}", error));
                return;
            }
            Console.WriteLine(String.Format("[SocketListener] Starting Service Thread"));

            Thread serviceThread = new Thread(() => ServiceThread());
            serviceThread.Start();

        }

        static void ServiceThread()
        {
            while(true)
            {
                try
                {
                    Socket clientConnection = listenerSocket.Accept();
                    IPEndPoint clientEndpoint = (IPEndPoint)clientConnection.RemoteEndPoint;

                    ConnectionStructure connectionStructure = new ConnectionStructure(clientConnection, clientEndpoint);
                    Console.WriteLine(String.Format("[SocketListener] New Connection " + connectionStructure.Socket.Handle));

                    Thread _tmpClientInstance = new Thread(() => connectionStructure.ThreadHandler());
                    _tmpClientInstance.Start();
                } catch (Exception error)
                {
                    Console.WriteLine("[BackendService] " + error.Message);
                }
            }
        }
    }
}

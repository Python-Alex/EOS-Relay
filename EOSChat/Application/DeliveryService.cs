using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EOSChat.Application
{
    public static class DeliveryService
    {
        public static Thread clientListDeliveryThread;

        public static void StartService()
        {
            clientListDeliveryThread = new Thread(() => ClientListSender());
            clientListDeliveryThread.Start();
        }
        public static void ClientListSender()
        {
            while(true)
            {
                string currentClientFrame = "";
                
                
                foreach(ClientStructure clientStructure in ActiveClientStructure.clientStructures)
                {
                    currentClientFrame += $"{clientStructure.Id}{clientStructure.Username};";
                }

                List<ClientStructure> badClients = new List<ClientStructure>();

                foreach (ClientStructure clientStructure in ActiveClientStructure.clientStructures)
                {
                    try
                    {
                        clientStructure.Connection.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.CLIENT_LIST_UPDATE, currentClientFrame, clientStructure.Id)));
                    } catch (Exception)
                    {
                        badClients.Add(clientStructure);
                    }
                    
                }

                while (ThreadPersistance.RemovingClientCallWaiting)
                    Thread.Sleep(10);

                ThreadPersistance.RemovingClientCallWaiting = true;
                foreach(ClientStructure clientStructure in badClients)
                {
                    ActiveClientStructure.clientStructures.Remove(clientStructure);
                }
                ThreadPersistance.RemovingClientCallWaiting = false;

                Thread.Sleep(5000);
            }
        }
    }
}

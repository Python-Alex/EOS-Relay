using System;
using System.Text;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Sockets;

namespace EOSChat
{
    public class ConnectionStructure
    {
        // transforms into client structure after login/regster process

        public Socket Socket;
        public IPEndPoint IpEndPoint;
        public ConnectionState connectionState = ConnectionState.CONNECTION_STATE;
        public ClientStructure clientStructure;

        public ConnectionStructure(Socket socket, IPEndPoint ipendpoint)
        {
            this.Socket = socket;
            this.IpEndPoint = ipendpoint;
        }

        public void ThreadHandler()
        {
            while(true)
            {
                byte[] receivedData = new Byte[2048];
                int receivedBytes = 0;

                try
                {
                    receivedBytes = this.Socket.Receive(receivedData);
                    Application.ResourceMeters.BytesReceivedTotal += receivedBytes;

                } catch (Exception error)
                {
                    if (clientStructure is not null)
                    {
                        while (ThreadPersistance.RemovingClientCallWaiting)
                            Thread.Sleep(10);

                        ThreadPersistance.RemovingClientCallWaiting = true;
                        ActiveClientStructure.clientStructures.Remove(clientStructure);
                        ThreadPersistance.RemovingClientCallWaiting = false;
                    }
                        
                    Console.WriteLine("[ConnectionStructure] Connection Error {0} | {1}", this.Socket.Handle, error.Message);
                    return;
                }

                string stringContent = Encoding.ASCII.GetString(receivedData, 0, receivedBytes);
                if (receivedBytes == 0 || receivedBytes == 2)
                    continue;

                dynamic payloadObject = null;

                try
                {
                    payloadObject = JsonConvert.DeserializeObject(stringContent);

                    Console.WriteLine(payloadObject.ToString());

                    EventFlag eventFlag = (EventFlag)payloadObject.flag;
                    string eventContent = (string)payloadObject.content;
                    string clientId = (string)payloadObject.clientId;

                    if (clientId is null)
                        clientId = "";

                    ExecutePayload(eventFlag, eventContent, clientId);
                
                } catch (Exception error)
                {
                    Console.WriteLine("[ConnectionStructure] Data Error, " + error.Message);
                }


            }
        }
        public bool ExecutePayload(EventFlag eventFlag, string content, string id)
        {

            if(connectionState == ConnectionState.CONNECTION_STATE)
            {
                Action<ConnectionStructure, string, string> action;
                ConnectionEventReference.PayloadExecuteTable.TryGetValue(eventFlag, out action);

                action(this, content, id);
                return true; 
            }


            else if(connectionState == ConnectionState.CLIENT_STATE)
            {
                Action<ClientStructure, string, string> action;
                EventReference.PayloadExecuteTable.TryGetValue(eventFlag, out action);

                action(clientStructure, content, id);
                return true;
            }

            return false;
        }
    }
}


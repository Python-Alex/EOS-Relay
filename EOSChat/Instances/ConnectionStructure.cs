using System;
using System.Text;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Sockets;

namespace EOSChat
{
    // IDisposable so we can properly cleanup this class, preventing possible memory leaks
    public class ConnectionStructure
    {
        // references into client structure after login/regster process

        public Socket Socket;
        public IPEndPoint IpEndPoint;
        public ConnectionState connectionState = ConnectionState.CONNECTION_STATE;
        public ClientStructure clientStructure;
        public int PacketsReceived = 0;
        public int TotalBytesReceived = 0;
        public bool RestrictedReceive = false;

        public ConnectionStructure(Socket socket, IPEndPoint ipendpoint)
        {
            this.Socket = socket;
            this.IpEndPoint = ipendpoint;
        }

        public void ThreadHandler()
        {
            while(true)
            {
                byte[] receivedData = new Byte[Application.ResourceLimit.MaxDataReceive];
                int receivedBytes = 0;

                try
                {
                    receivedBytes = this.Socket.Receive(receivedData);
                    Application.ResourceMeters.BytesReceivedTotal += receivedBytes;
                    TotalBytesReceived += receivedBytes;
                    
                    

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

                if (RestrictedReceive)
                    continue;

                string stringContent = Encoding.ASCII.GetString(receivedData, 0, receivedBytes);
                if (receivedBytes == 0 || receivedBytes == 2)
                    continue;

                dynamic payloadObject = null;

                try
                {
                    payloadObject = JsonConvert.DeserializeObject(stringContent);
                    Console.WriteLine(payloadObject.ToString());
                    PacketsReceived += 1;

                    EventFlag eventFlag = (EventFlag)payloadObject.flag;
                    string eventContent = (string)payloadObject.content;
                    string clientId = (string)payloadObject.clientId;

#if DEBUG
                    Console.WriteLine($"[ConnectionStructure] Parsed Payload with Flag " + eventFlag.ToString());
#endif

                    if (clientId is null)
                        clientId = "";

                    ExecutePayload(eventFlag, eventContent, clientId);
                
                } catch (Exception error)
                {
                    Console.WriteLine("[ConnectionStructure] Data Error, " + error.Message + " " + error.TargetSite.Name);
                }


            }
        }
        public bool ExecutePayload(EventFlag eventFlag, string content, string id)
        {

            if(connectionState == ConnectionState.CONNECTION_STATE)
            {
                Action<ConnectionStructure, string, string> action;
                ConnectionEventReference.PayloadExecuteTable.TryGetValue(eventFlag, out action);

                if (action is null)
                {
                    Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.UNAUTHORIZED_REQUEST, "invalid context", id)));
                    return false;
                }    
                    

                try
                {
                    action(this, content, id);
                } catch (Exception error)
                {
                    Console.WriteLine("[ConnectionStructure] Execute Error, " + error.Message + " " + error.StackTrace);
                }
                return true; 
            }


            else if(connectionState == ConnectionState.CLIENT_STATE)
            {
                Action<ClientStructure, string, string> action;
                EventReference.PayloadExecuteTable.TryGetValue(eventFlag, out action);

                if (action is null)
                {
                    EventReference.SendPayload(clientStructure, EventReference.CreatePayload(EventFlag.UNAUTHORIZED_REQUEST, "invalid context", id));
                    return false;
                }

                try
                {
                    action(clientStructure, content, id);
                }
                catch (Exception error)
                {
                    Console.WriteLine("[ConnectionStructure] Execute Error, " + error.Message + " " + error.StackTrace);
                }
                return true;
            }

            return false;
        }
    }
}


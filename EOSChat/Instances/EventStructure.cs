using System;
using System.Text;
using System.Net.Mail;
using System.Collections.Generic;

namespace EOSChat
{
    [Flags]
    public enum EventFlag
    {
        CLIENT_MESSAGE_SEND = 1, // done - TESTED
        CLIENT_MESSAGE_RECEIVE = 2, // done - TESTED

        CLIENT_UPDATE_USERNAME = 3, // done - TESTED
        CLIENT_UPDATE_EMAIL = 4, // done - TESTED
        CLIENT_UPDATE_PASSWORD = 5, // done - TESTED
        
        CLIENT_FRIEND_INVITE = 6, // done - UNTESTED
        CLIENT_FRIEND_REMOVE = 7,// done - UNTESTED
        CLIENT_FRIEND_ACCEPT = 8, // done - UNTESTED
        CLIENT_FRIEND_DENY = 9, // done - UNTESTED

        CLIENT_IGNORE_CLIENT = 10, // done - TESTED
        CLIENT_REMOVE_IGNORE = 11, // done - UNTESTED
        
        INVOKE_CLIENT_NOTIFICATION = 12,

        CLIENT_REGISTER_PROFILE = 13, // done - TESTED
        CLIENT_LOGIN_PROFILE = 14, // done -TESTED

        GENERAL_ERROR = 15,
        UPDATE_ERROR = 16,
        RATELIMIT_ERROR = 17,
        UPDATE_SUCCESSFUL = 18,

        CLIENT_LIST_UPDATE = 19, // DeliveryService.cs  done - T

        LOGIN_STATE_SUCCESSFUL = 98, 
        LOGIN_STATE_FAILURE = 99,

        REGISTERING_STATE_PROCESSING = 100,
        REGISTERING_STATE_VALIDATING = 101,
        REGISTERING_STATE_SUCCEEDED = 102,
        REGISTERING_STATE_FAILED = 103,

        UNAUTHORIZED_REQUEST = 0xFFFF,
    }


    public enum ConnectionState
    {
        CONNECTION_STATE = 0,
        CLIENT_STATE = 1
    }

    public static class ConnectionEventReference
    {
        public static Dictionary<EventFlag, Action<ConnectionStructure, string, string>> PayloadExecuteTable = new()
        {
            { EventFlag.CLIENT_REGISTER_PROFILE, (ConnectionStructure a, string b, string c) => Event_ClientRegisterProfile(a, b, c) },
            { EventFlag.CLIENT_LOGIN_PROFILE, (ConnectionStructure a, string b, string c) => Event_ClientProfileLogin(a, b, c) }
        };

        static void Event_ClientProfileLogin(ConnectionStructure connectionStructure, string eventContent, string clientId)
        {
            string usernameOrEmail;
            string passwordHash;

            string[] splitContent = eventContent.Split(";");

            if (splitContent.Length < 2)
            {
                connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.LOGIN_STATE_FAILURE, "failed to login", clientId)));
                return;
            }

            foreach(ClientStructure clientStructures in ActiveClientStructure.clientStructures)
            {
                if(clientStructures.Id == clientId)
                {
                    connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.LOGIN_STATE_FAILURE, "failed to login", clientId)));
                    return;
                }
            }

            usernameOrEmail = splitContent[0];
            passwordHash = splitContent[1];

            bool valid = MailAddress.TryCreate(usernameOrEmail, out MailAddress tmp);

            dynamic credentials = FileCredentials.CredentialReader.ReadCredentialFile(clientId);

            if (credentials is null)
            {
                connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.LOGIN_STATE_FAILURE, "failed to login", clientId)));
                return;
            }

            bool userAuthValid = false;
            bool passwordValid = false;
            

            if (!valid)
                if (credentials.username == usernameOrEmail)
                    userAuthValid = true;
                //
            else if (valid)
                if (credentials.email == usernameOrEmail)
                    userAuthValid = false;

            if (credentials.password == passwordHash)
                passwordValid = true;


            if (userAuthValid && passwordValid)
            {
                ClientStructure clientStructure = new ClientStructure(connectionStructure, (string)credentials.username, (string)credentials.email, (string)credentials.password, (string)credentials.id);
                connectionStructure.clientStructure = clientStructure;
                connectionStructure.connectionState = ConnectionState.CLIENT_STATE;
                connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.LOGIN_STATE_SUCCESSFUL, "logged in", clientId)));
             
                Console.WriteLine("[ConnectionStructure] Switching Payload Execution Method, Client Logged In");

            }
        }

        static void Event_ClientRegisterProfile(ConnectionStructure connectionStructure, string eventContent, string clientId)
        {
            connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.REGISTERING_STATE_PROCESSING, "", "")));

            string username;
            string passwordHash;
            string email;


            string[] splitContent = eventContent.Split(";");

            if(splitContent.Length < 3)
            {
                connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.REGISTERING_STATE_FAILED, "failed to create profile", "")));
                return;
            }

            username = splitContent[0];
            passwordHash = splitContent[1];
            email = splitContent[2];

            foreach(string directory in System.IO.Directory.EnumerateDirectories(FileCredentials.CredentialReader.FileCredentialPath))
            {
                dynamic credentials = FileCredentials.CredentialReader.ReadCredentialFile(directory);
                if (credentials != null && credentials.Username == username)
                {
                    connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.REGISTERING_STATE_FAILED, "failed to create profile", "")));
                    return;
                }

                if(credentials != null && credentials.Email == email)
                {
                    connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.REGISTERING_STATE_FAILED, "failed to create profile", "")));
                    return;
                }
            }
            if (username.Length > 24)
            {
                connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.REGISTERING_STATE_FAILED, "username length exceeded 24", "")));
                return;
            }

            bool mailCreated = MailAddress.TryCreate(email, out MailAddress tmp);
            if (!mailCreated)
            {
                connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.REGISTERING_STATE_FAILED, "email was invalid", "")));
                return;
            }
            connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.REGISTERING_STATE_VALIDATING, "", "")));

            Guid newGuid = Guid.NewGuid();

            bool created = FileCredentials.CredentialWriter.NewCredentials(newGuid.ToString(), username, passwordHash, email);
            if(!created)
            {
                connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.REGISTERING_STATE_FAILED, "failed to create profile", "")));
                return;
            }

            connectionStructure.Socket.Send(Encoding.ASCII.GetBytes(EventReference.CreatePayload(EventFlag.REGISTERING_STATE_SUCCEEDED, "", newGuid.ToString())));

            connectionStructure.clientStructure = new ClientStructure(connectionStructure, username, email, passwordHash, newGuid.ToString());

            connectionStructure.connectionState = ConnectionState.CLIENT_STATE;
            Console.WriteLine("[ConnectionStructure] Switching Payload Execution Method, Client Registered New Profile");

            return;

        }
    }

    public static class EventReference
    {
        public static Dictionary<EventFlag, Action<ClientStructure, string, string>> PayloadExecuteTable = new()
        {
            { EventFlag.CLIENT_MESSAGE_SEND,    (ClientStructure a, string b, string c) => Event_ClientMessageSend(a, b, c) },
            { EventFlag.CLIENT_UPDATE_EMAIL,    (ClientStructure a, string b, string c) => Event_ClientUpdateEmail(a, b, c) },
            { EventFlag.CLIENT_UPDATE_USERNAME, (ClientStructure a, string b, string c) => Event_ClientUpdateUsername(a, b, c) },
            { EventFlag.CLIENT_UPDATE_PASSWORD, (ClientStructure a, string b, string c) => Event_ClientUpdatePassword(a, b, c) },
            { EventFlag.CLIENT_FRIEND_INVITE,   (ClientStructure a, string b, string c) => Application.Friends.InviteClient(a, b, c) },
            { EventFlag.CLIENT_FRIEND_REMOVE,   (ClientStructure a, string b, string c) => Application.Friends.RemoveClient(a, b, c) },
            { EventFlag.CLIENT_FRIEND_ACCEPT,   (ClientStructure a, string b, string c) => Application.Friends.AcceptInvite(a, b, c) },
            { EventFlag.CLIENT_FRIEND_DENY,     (ClientStructure a, string b, string c) => Application.Friends.CancelInvite(a, b, c) },
            { EventFlag.CLIENT_IGNORE_CLIENT,   (ClientStructure a, string b, string c) => Application.Ignored.IgnoreClient(a, b, c) },
            { EventFlag.CLIENT_REMOVE_IGNORE,   (ClientStructure a, string b, string c) => Application.Ignored.RemoveClient(a, b, c) },
        };

        static void Event_ClientMessageSend(ClientStructure clientStructure, string eventContent, string clientId)
        {
            if (clientStructure.Id == clientId)
                return;

            ClientStructure receivingClient = ActiveClientStructure.FromId(clientId);
            if (receivingClient is null)
                return;

            string receiveMessagePayload = CreatePayload(EventFlag.CLIENT_MESSAGE_RECEIVE, eventContent, clientStructure.Id.ToString());
            SendPayload(receivingClient, receiveMessagePayload);
        }

        static void Event_ClientUpdateUsername(ClientStructure clientStructure, string eventContext, string clientId)
        {
            Console.WriteLine(clientId is null);
            if (eventContext.Length > 24)
            {
                SendPayload(clientStructure, CreatePayload(EventFlag.UPDATE_ERROR, "username was too long", clientId));
                return;
            }

            else if(eventContext.Length < 2)
            {
                SendPayload(clientStructure, CreatePayload(EventFlag.UPDATE_ERROR, "username was too short", clientId));
                return;
            }

            clientStructure.Username = eventContext;
            FileCredentials.CredentialWriter.UpdateCredentialFile(clientId, FileCredentials.UpdateFlag.UPDATE_USERNAME, eventContext);
            SendPayload(clientStructure, CreatePayload(EventFlag.UPDATE_SUCCESSFUL, "updated username", clientId));
        }

        static void Event_ClientUpdateEmail(ClientStructure clientStructure, string eventContent, string clientId)
        {
            bool mailCreated = MailAddress.TryCreate(eventContent, out MailAddress tmp);
            if (!mailCreated)
            {
                SendPayload(clientStructure, CreatePayload(EventFlag.UPDATE_ERROR, "email was invalid", clientId));
                return;
            }
            clientStructure.Email = eventContent;
            FileCredentials.CredentialWriter.UpdateCredentialFile(clientId, FileCredentials.UpdateFlag.UPDATE_EMAIL, eventContent);
            SendPayload(clientStructure, CreatePayload(EventFlag.UPDATE_SUCCESSFUL, "updated email address", clientId));
        }

        static void Event_ClientUpdatePassword(ClientStructure clientStructure, string eventContent, string clientId)
        {
            clientStructure.Password = eventContent;
            FileCredentials.CredentialWriter.UpdateCredentialFile(clientId, FileCredentials.UpdateFlag.UPDATE_PASSWORD, eventContent);
            SendPayload(clientStructure, CreatePayload(EventFlag.UPDATE_SUCCESSFUL, "updated password", clientId));
        }

        public static string CreatePayload(EventFlag flag, string eventContent, string clientId)
        {
            return "{ \"flag\": " + (int)flag +", \"content\": \"" + eventContent + "\", \"clientid\": \"" + clientId + "\" }";
        }

        public static void SendPayload(ClientStructure clientStructure, string payload)
        {
            clientStructure.Connection.Socket.Send(Encoding.ASCII.GetBytes(payload));
        }
    }
}

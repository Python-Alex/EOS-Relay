using System;
using System.Collections.Generic;

namespace EOSChat
{
    public static class ActiveClientStructure
    {
        public static List<ClientStructure> clientStructures = new List<ClientStructure>();

        public static ClientStructure FromId(string id)
        {
            foreach(ClientStructure clientStructure in clientStructures)
            {
                if (clientStructure.Id.ToString() == id)
                    return clientStructure;
            }

            return null;
        }

        public static ClientStructure FromUsername(string username)
        {
            foreach(ClientStructure clientStructure in clientStructures)
            {
                if (clientStructure.Username == username)
                    return clientStructure;
            }

            return null;
        }

        [Obsolete]
        public static ClientStructure FromAddress(string ipv4)
        {
            foreach(ClientStructure clientStructure in clientStructures)
            {
                if (clientStructure.Connection.IpEndPoint.Address.Address.Equals(ipv4))
                    return clientStructure;
            }
            return null;
        }
    }

    public class ClientStructure
    {
        public string Id;
        public string Username;
        public string Email;
        public string Password;
        public ConnectionStructure Connection;

        public ClientStructure(ConnectionStructure connection, string username, string email, string password, string id)
        {
            this.Connection = connection;
            this.Username = username;
            this.Email = email;
            this.Password = password;
            this.Id = id;

            ActiveClientStructure.clientStructures.Add(this);
        }


    }
}

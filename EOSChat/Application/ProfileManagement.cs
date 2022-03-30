using System.Collections.Generic;


namespace EOSChat.Application
{
    public static class Ignored
    {
        // EventReference Table Function
        public static void IgnoreClient(ClientStructure clientStructure, string eventContent, string clientId)
        {

            List<string> ignoredClients = FileCredentials.CredentialReader.ReadClientIgnoreList(clientStructure.Id);
            if(ignoredClients.Contains(clientId))
            {
                EventReference.SendPayload(clientStructure, EventReference.CreatePayload(EventFlag.GENERAL_ERROR, "client already ignored", clientId));
                return;
            }

            FileCredentials.CredentialWriter.NewIgnoredClient(clientStructure.Id, clientId);

        }

        public static void RemoveClient(ClientStructure clientStructure, string eventContent, string clientId)
        {
            List<string> ignoredClients = FileCredentials.CredentialReader.ReadClientIgnoreList(clientStructure.Id);
            if(!ignoredClients.Contains(clientId))
            {
                EventReference.SendPayload(clientStructure, EventReference.CreatePayload(EventFlag.GENERAL_ERROR, "client is not ignored", clientId));
                return;
            }

            FileCredentials.CredentialWriter.RemoveIgnoredClient(clientStructure.Id, clientId);
        }
    }

    public static class Friends
    {
        public static void InviteClient(ClientStructure clientStructure, string eventContent, string clientId)
        {
            List<string> pendingInvites = FileCredentials.CredentialReader.ReadClientPendingInvites(clientStructure.Id);
            if (pendingInvites.Contains(clientId))
            {
                EventReference.SendPayload(clientStructure, EventReference.CreatePayload(EventFlag.GENERAL_ERROR, "invite already pending", clientId));
                return;
            }

            FileCredentials.CredentialWriter.NewFriendInvite(clientStructure.Id, clientId);
        }

        public static void CancelInvite(ClientStructure clientStructure, string eventContent, string clientId)
        {
            List<string> pendingInvites = FileCredentials.CredentialReader.ReadClientPendingInvites(clientStructure.Id);
            if (!pendingInvites.Contains(clientId))
            {
                EventReference.SendPayload(clientStructure, EventReference.CreatePayload(EventFlag.GENERAL_ERROR, "no pending invite", clientId));
                return;
            }

            FileCredentials.CredentialWriter.RemoveFriendInvite(clientStructure.Id, clientId);
        }

        public static void RemoveClient(ClientStructure clientStructure, string eventContent, string clientId)
        {
            List<string> friendlist = FileCredentials.CredentialReader.ReadClientFriendsList(clientStructure.Id);
            if (!friendlist.Contains(clientId))
            {
                EventReference.SendPayload(clientStructure, EventReference.CreatePayload(EventFlag.GENERAL_ERROR, "client is not your friend", clientId));
                return;
            }

            FileCredentials.CredentialWriter.RemoveFriendClient(clientStructure.Id, clientId);
        }

        public static void AcceptInvite(ClientStructure clientStructure, string eventContent, string clientId)
        {
            List<string> pendingInvites = FileCredentials.CredentialReader.ReadClientPendingInvites(clientStructure.Id);
            if (!pendingInvites.Contains(clientId))
            {
                EventReference.SendPayload(clientStructure, EventReference.CreatePayload(EventFlag.GENERAL_ERROR, "no pending invite", clientId));
                return;
            }
            FileCredentials.CredentialWriter.NewFriendClient(clientStructure.Id, clientId);
        }
    }
}

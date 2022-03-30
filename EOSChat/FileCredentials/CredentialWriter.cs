using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EOSChat.FileCredentials
{
    public enum UpdateFlag
    {
        UPDATE_USERNAME = 0,
        UPDATE_PASSWORD = 1,
        UPDATE_EMAIL = 2
    }
    public static class CredentialWriter
    {
        public static bool WritingContent = false;
        public static string FileCredentialPath = AppDomain.CurrentDomain.BaseDirectory + "FileCredentials";

        public static void SetupCredentialStaticFiles()
        {
            System.IO.Directory.CreateDirectory(FileCredentialPath);
        }

        public static void RemoveIgnoredClient(string id, string ignored)
        {
            string[] lines = File.ReadAllLines(FileCredentialPath + @"\" + id + @"\ignorelist");
            List<string> nlines = new List<string>(lines);
            nlines.Remove(ignored);
            File.WriteAllLines(FileCredentialPath + @"\" + id + @"\ignorelist", nlines.ToArray());
        }

        public static void RemoveFriendClient(string id, string friend)
        {
            string[] lines = File.ReadAllLines(FileCredentialPath + @"\" + id + @"\friendlist");
            List<string> nlines = new List<string>(lines);
            nlines.Remove(friend);
            File.WriteAllLines(FileCredentialPath + @"\" + id + @"\friendlist", nlines.ToArray());
        }

        public static void RemoveFriendInvite(string id, string friend)
        {
            string[] lines = File.ReadAllLines(FileCredentialPath + @"\" + id + @"\pendinginvites");
            List<string> nlines = new List<string>(lines);
            nlines.Remove(friend);
            File.WriteAllLines(FileCredentialPath + @"\" + id + @"\pendinginvites", nlines.ToArray());
        }

        public static void NewIgnoredClient(string id, string ignored)
        {
            using (StreamWriter sw = File.AppendText(FileCredentialPath + @"\" + id + @"\ignorelist"))
            {
                sw.WriteLine(ignored);
            }
        }

        public static void NewFriendClient(string id, string friend)
        {
            // client accepted friend invite fron 'friend'
            using (StreamWriter sw = File.AppendText(FileCredentialPath + @"\" + id + @"\friendlist"))
            {
                sw.WriteLine(friend);
            }
            RemoveFriendInvite(id, friend);
        }

        public static void NewFriendInvite(string id, string friend)
        {
            // client sent friend invite
            using (StreamWriter sw = File.AppendText(FileCredentialPath + @"\" + id + @"\pendinginvites"))
            {
                sw.WriteLine(friend);
            }
            
        }

        public static bool UpdateCredentialFile(string id, UpdateFlag identifier, string value)
        {
            if (!System.IO.Directory.Exists(FileCredentialPath + @"\" + id))
                return false;

            CredentialReader.ReadingContent = true;

            dynamic credentialsObject = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(FileCredentialPath + @"\" + id + @"\credentials.json"));

            CredentialReader.ReadingContent = false;

            switch (identifier)
            {
                case UpdateFlag.UPDATE_USERNAME:
                    credentialsObject.username = value;
                    break;

                case UpdateFlag.UPDATE_PASSWORD:
                    credentialsObject.password = value;
                    break;

                case UpdateFlag.UPDATE_EMAIL:
                    credentialsObject.email = value;
                    break;
            }

            WritingContent = true;
            
            System.IO.File.WriteAllText(
                FileCredentialPath + @"\" + id + @"\credentials.json", JsonConvert.SerializeObject(credentialsObject)
            ) ;

            WritingContent = false;

            return true;

        }

        public static bool NewCredentials(string id, string username, string passwordHash, string email)
        {
            
            if (System.IO.Directory.Exists(FileCredentialPath + @"\" + id))
                return false;

            System.IO.Directory.CreateDirectory(FileCredentialPath + @"\" + id);

            string jsonString = "{\"id\": \"" + id + "\", \"username\": \"" + username + "\", \"password\": \"" + passwordHash + "\", \"email\": \"" + email + "\"}";

            WritingContent = true;

            System.IO.File.WriteAllText(FileCredentialPath + @"\" + id + @"\credentials.json", jsonString);

            System.IO.File.Create(
                FileCredentialPath + @"\" + id + @"\friendlist"
            );

            System.IO.File.Create(
                FileCredentialPath + @"\" + id + @"\ignorelist"
            );

            System.IO.File.Create(
                FileCredentialPath + @"\" + id + @"\pendinginvites"
            );

            WritingContent = false;

            return true;
        }
    }
}

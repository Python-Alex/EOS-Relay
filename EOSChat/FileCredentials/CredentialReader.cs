using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EOSChat.FileCredentials
{
    public static class CredentialReader
    {
        public static bool ReadingContent = false;
        public static string FileCredentialPath = AppDomain.CurrentDomain.BaseDirectory + "FileCredentials";

        public static dynamic ReadCredentialFile(string id)
        {
            if(System.IO.Directory.Exists(FileCredentialPath + @"\" + id))
            {
                //Console.WriteLine("[CredentialReader] Reading Credentials File - " + FileCredentialPath + @"\" + id + @"\credentials.json");
                if(System.IO.File.Exists(FileCredentialPath + @"\" + id + @"\credentials.json"))
                {
                    ReadingContent = true;
                    dynamic jsonObject = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(FileCredentialPath + @"\" + id + @"\credentials.json"));
                    ReadingContent = false;

                    return jsonObject;
                }
                
            }

            return null; 

        }

        public static List<string> ReadClientFriendsList(string id)
        {
            List<string> results = new List<string>();
            if (!System.IO.Directory.Exists(FileCredentialPath + @"\" + id))
                return null;

            foreach(string line in System.IO.File.ReadAllLines(FileCredentialPath + @"/" + id + @"\friendlist"))
            {
                results.Add(line);
            }

            return results;
        }

        public static List<string> ReadClientIgnoreList(string id)
        {
            List<string> results = new List<string>();
            if (!System.IO.Directory.Exists(FileCredentialPath + @"\" + id))
                return null;

            foreach (string line in System.IO.File.ReadAllLines(FileCredentialPath + @"/" + id + @"\ignorelist"))
            {
                results.Add(line);
            }

            return results;
        }

        public static List<string> ReadClientPendingInvites(string id)
        {
            List<string> results = new List<string>();
            if (!System.IO.Directory.Exists(FileCredentialPath + @"\" + id))
                return null;

            foreach (string line in System.IO.File.ReadAllLines(FileCredentialPath + @"/" + id + @"\pendinginvites"))
            {
                results.Add(line);
            }

            return results;
        }

    }
}

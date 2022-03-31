using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EOSChat.Config
{
    public static class ConfigurationReader
    {
        public static string ConfigurationDirectory = AppDomain.CurrentDomain.BaseDirectory + "Config";
        public static dynamic EncryptionConfiguration;
        public static dynamic NetworkConfiguration;

        private static string BaseNetworkConfigString = "{\n\t\"MaxPayloadsPerSecond\": 200,\n\t\"MaxDataReceive\": 2048,\n\t\"MaxClientTimeout\": 3600\n}";
        private static string BaseEncryptionConfString = "{\n\t\"ConnectionStructureMethods\": [],\n\t\"ClientStructureMethods\": []\n}";
        public static void FirstTimeSetup()
        {
            if (Directory.Exists(ConfigurationDirectory))
                return;

            Directory.CreateDirectory(ConfigurationDirectory);
            File.WriteAllText(ConfigurationDirectory + @"\Network.json", BaseNetworkConfigString);
            File.WriteAllText(ConfigurationDirectory + @"\Encryption.json", BaseEncryptionConfString);

        }

        public static void SetAutomaticConfigurations()
        {
            FirstTimeSetup();

            EncryptionConfiguration = JsonConvert.DeserializeObject(File.ReadAllText(ConfigurationDirectory + @"\Encryption.json"));
            NetworkConfiguration = JsonConvert.DeserializeObject(File.ReadAllText(ConfigurationDirectory + @"\Network.json"));

            Application.ResourceLimit.MaxDataReceive = NetworkConfiguration.MaxDataReceive;
            Application.ResourceLimit.MaxPayloadReceivesPerMinute = NetworkConfiguration.MaxPayloadsPerSecond * 60;

            Application.ResourceMeters.MaxBandwidthPerClient = (Int64)(((Application.ResourceLimit.MaxPayloadReceivesPerMinute / 60) * Application.ResourceLimit.MaxDataReceive) / 1024);
            Console.Title = $"EOS-Relay | Max Bandwidth Acceptable: {Application.ResourceMeters.MaxBandwidthPerClient * ActiveClientStructure.clientStructures.Count()}KBps," +
                $" Max Payloads Per Second: {(Application.ResourceLimit.MaxPayloadReceivesPerMinute / 60) * ActiveClientStructure.clientStructures.Count()}, Clients: {ActiveClientStructure.clientStructures.Count()} " +
                $"Total Traffic: { (float)(Application.ResourceMeters.BytesReceivedTotal / 1024) }KB";
            
        }
    }
}

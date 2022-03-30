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
        public static string ResourceLimitCfgFile = AppDomain.CurrentDomain.BaseDirectory + "Config";
        public static dynamic EncryptionConfiguration = JsonConvert.DeserializeObject(File.ReadAllText(ResourceLimitCfgFile + @"\Encryption.json"));
        public static dynamic NetworkConfiguration = JsonConvert.DeserializeObject(File.ReadAllText(ResourceLimitCfgFile + @"\Network.json"));

        public static void SetAutomaticConfigurations()
        {
            Application.ResourceLimit.MaxDataReceive = NetworkConfiguration.MaxBufferReceive;
            Application.ResourceLimit.MaxPayloadReceivesPerMinute = NetworkConfiguration.MaxPayloadsPerSecond * 60;

            Application.ResourceMeters.MaxBandwidthPerClient = (((Application.ResourceLimit.MaxPayloadReceivesPerMinute / 60) * Application.ResourceLimit.MaxDataReceive) / 1024);
        }
    }
}

using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSChat.Application
{
    public static class ResourceMeters
    {
        public static Int64 BytesReceivedTotal = 0;
        public static Int64 BytesReceivedBefore = 0;
        public static Int64 ConnectionsReceivedLastHour = 0;
        public static Int64 _ConnectionTimerTick = 0;
        public static Int64 _DataCheckTimerTick = 0;
        public static Timer Timer;

        public static void StartResourcing()
        {
            Timer = new Timer(1000);
            Timer.Elapsed += OnTimedEvent;
            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        public static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (_ConnectionTimerTick == 3600)
            {
                _ConnectionTimerTick = 0;
                if (ActiveClientStructure.clientStructures.Count() > ConnectionsReceivedLastHour)
                    ConnectionsReceivedLastHour = ActiveClientStructure.clientStructures.Count() - ConnectionsReceivedLastHour;
                else
                    ConnectionsReceivedLastHour = ConnectionsReceivedLastHour - ActiveClientStructure.clientStructures.Count();
            }

            if(_DataCheckTimerTick == 60)
            {
                _DataCheckTimerTick = 0;
                foreach(ClientStructure clientStructure in ActiveClientStructure.clientStructures)
                {
                    if (clientStructure.Connection.RestrictedReceive)
                        clientStructure.Connection.RestrictedReceive = false;
                }
            }

            foreach (ClientStructure clientStructure in ActiveClientStructure.clientStructures)
            {
                if (clientStructure.Connection.PacketsReceived > ResourceLimit.MaxPayloadReceivesPerMinute)
                {
                    if(clientStructure.Connection.RestrictedReceive)
                        continue;
                    
                    clientStructure.Connection.RestrictedReceive = true;
                    EventReference.SendPayload(clientStructure, EventReference.CreatePayload(EventFlag.RATELIMIT_ERROR, $"you are rate limited for {60 - _DataCheckTimerTick} seconds", clientStructure.Id));
                }
            }

            BytesReceivedBefore = BytesReceivedTotal - BytesReceivedBefore;

            Console.WriteLine($"[ResourceMeter] Bytes Received Last 10 Sec: {BytesReceivedBefore} - RAM Usage: {System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024}MB");

            _ConnectionTimerTick++;
            _DataCheckTimerTick++;
        }
    }

    public static class ResourceLimit
    {
        public static Int64 MaxPayloadReceivesPerMinute = 600; // 600 pps
        public static Int64 MaxDataReceivePerMinute = MaxPayloadReceivesPerMinute * 600; // ~351kbps
    }
}

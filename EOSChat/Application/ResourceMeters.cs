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
        public static int BytesReceivedTotal = 0;
        public static int BytesReceivedBefore = 0;
        public static int ConnectionsReceivedLastHour = 0;
        public static int _ConnectionTimerTick = 0;
        public static Timer Timer;

        public static void StartResourcing()
        {
            Timer = new Timer(60000);
            Timer.Elapsed += OnTimedEvent;
            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        public static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if(_ConnectionTimerTick == 1)
            {
                _ConnectionTimerTick = 0;
                if (ActiveClientStructure.clientStructures.Count() > ConnectionsReceivedLastHour)
                    ConnectionsReceivedLastHour = ActiveClientStructure.clientStructures.Count() - ConnectionsReceivedLastHour;
                else
                    ConnectionsReceivedLastHour = ConnectionsReceivedLastHour - ActiveClientStructure.clientStructures.Count();
            }
            BytesReceivedBefore = BytesReceivedTotal - BytesReceivedBefore;

            Console.WriteLine($"[ResourceMeter] Bytes Received Last 10 Sec: {BytesReceivedBefore} - RAM Usage: {System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024}MB");

            _ConnectionTimerTick++;
        }
    }
}

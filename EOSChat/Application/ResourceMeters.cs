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
        public static int BytesReceivedPerSecond = 0;
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

        }
    }
}

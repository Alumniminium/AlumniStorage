using System;
using System.Threading;

namespace Universal.IO.Sockets.Monitoring
{
    public static class NetworkMonitor
    {
        public static ulong BytesSent { get; private set; }
        public static ulong BytesReceived { get; private set; }
        public static float UploadSpeed { get; private set; }
        public static float DownloadSpeed { get; private set; }
        public static ulong PacketsReceived { get; private set; }
        public static ulong PacketsSent { get; private set; }

        public static float DownloadSpeedAverage => _lastTrafficIn / _counterSeconds;
        public static float UploadSpeedAverage => _lastTrafficOut / _counterSeconds;

        private static ulong _lastTrafficIn, _lastTrafficOut, _counterSeconds = 1;
        private static readonly System.Timers.Timer _bandwidthTimer = new System.Timers.Timer(1000);

        static NetworkMonitor()
        {
            _bandwidthTimer.Elapsed += (sender, args) =>
            {
                _counterSeconds++;
                UploadSpeed = BytesSent - _lastTrafficOut;
                DownloadSpeed = BytesReceived - _lastTrafficIn;

                _lastTrafficIn = BytesReceived;
                _lastTrafficOut = BytesSent;
                Console.Title = $"DL: {(DownloadSpeed / 1024f / 1024f):##0.00} MB/s (avg: {(DownloadSpeedAverage / 1024f / 1024f):##0.00})  UL: {(UploadSpeed / 1024 / 1024):##0.00} MB/s (avg: {(UploadSpeedAverage / 1024 / 1024):##0.00)}";
            };
            _bandwidthTimer.Enabled = true;
            _bandwidthTimer.Start();
            Thread.Sleep(1000);
        }

        public static void Log(int size, TrafficMode mode)
        {
            switch (mode)
            {
                case TrafficMode.In:
                    PacketsReceived++;
                    BytesReceived += (ulong)size;
                    break;
                case TrafficMode.Out:
                    PacketsSent++;
                    BytesSent += (ulong)size; break;
            }
        }

    }
}
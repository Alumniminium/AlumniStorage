using System;
using System.Threading;

namespace Universal.IO.Sockets.Monitoring
{
    public class NetworkMonitor
    {
        public ulong PacketsPerSecondOutgoing { get; private set; }
        public ulong PacketsPerSecondIncoming { get; private set; }
        public ulong BytesSent { get; private set; }
        public ulong BytesReceived { get; private set; }
        public ulong UploadSpeed { get; private set; }
        public ulong DownloadSpeed { get; private set; }
        public ulong PacketsReceived { get; private set; }
        public ulong PacketsSent { get; private set; }

        public ulong DownloadSpeedAverage => _lastTrafficIn / _counterSeconds;
        public ulong UploadSpeedAverage => _lastTrafficOut / _counterSeconds;

        private ulong _lastTrafficIn, _lastTrafficOut, _counterSeconds = 1;
        private readonly System.Timers.Timer _bandwidthTimer = new System.Timers.Timer(1000);
        private readonly System.Timers.Timer _packetPerSecondTimer = new System.Timers.Timer(1000);

        public NetworkMonitor()
        {
            _bandwidthTimer.Elapsed += (sender, args) =>
            {
                _counterSeconds++;
                UploadSpeed = BytesSent - _lastTrafficOut;
                DownloadSpeed = BytesReceived - _lastTrafficIn;

                _lastTrafficIn = BytesReceived;
                _lastTrafficOut = BytesSent;
            };
            _bandwidthTimer.Enabled = true;
            _bandwidthTimer.Start();
            Thread.Sleep(500);
            _packetPerSecondTimer.Elapsed += (sender, args) =>
            {
                Console.Title = $"Packets per sec: Out: {PacketsPerSecondOutgoing} In: {PacketsPerSecondIncoming} | DL: {DownloadSpeedAverage / 1024} UL: {UploadSpeedAverage / 1024}";
                PacketsPerSecondOutgoing = 0;
                PacketsPerSecondIncoming = 0;
            };
            _packetPerSecondTimer.Enabled = true;
            _packetPerSecondTimer.Start();
        }

        public void Log(int size, TrafficMode mode)
        {
            switch (mode)
            {
                case TrafficMode.In:
                    PacketsPerSecondIncoming++;
                    PacketsReceived++;
                    BytesReceived += (ulong)size;
                    break;
                case TrafficMode.Out:
                    PacketsPerSecondOutgoing++;
                    PacketsSent++;
                    BytesSent += (ulong)size; break;
            }
        }

    }
}
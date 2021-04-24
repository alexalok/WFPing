using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace WFPing
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            var socket = new Socket(SocketType.Dgram, ProtocolType.IP);

            var rnd = new Random();
            var data = new byte[64];


            var serverNum = 7;
            var portNum = 64101;
            string serverIp = $"178.162.220.{serverNum}";
            socket.Connect(serverIp, portNum);
            const int tries = 5;
            long rtt = 0;
            long minPing = 0;
            long maxPing = 0;
            long totalPing = 0;
            const long timeout = 5000;

            var droppedTimes = 0;
            for (var i = 0; i < tries; i++)
            {
                var isDropped = true;
                rnd.NextBytes(data);
                data[0] = 0x50;
                stopwatch.Start();
                socket.Send(data);
                while (stopwatch.ElapsedMilliseconds < timeout)
                {
                    while (socket.Available != 0)
                    {
                        var buffer = new byte[1024];
                        socket.Receive(buffer);
                        if (buffer[0] != 0x50 || buffer[1] != 0x4F) continue;
                        rtt = stopwatch.ElapsedMilliseconds;
                        if (rtt < minPing)
                            minPing = rtt;
                        if (rtt > maxPing)
                            maxPing = rtt;
                        totalPing += rtt;
                        isDropped = false;
                        stopwatch.Reset();
                    }
                    if (!isDropped)
                        break;
                }
                if (isDropped)
                {
                    droppedTimes++;
                }
                while (socket.Available != 0)
                {
                    socket.Receive(new byte[1]);
                }
                Console.WriteLine("64 bytes of data, RTT: " + rtt);
            }
            float avgPing = (float) totalPing / (tries - droppedTimes);
            Console.WriteLine("Min/Avg/Max: {0}/{1}/{2}", minPing, avgPing, maxPing);
            Console.WriteLine("Dropped: " + droppedTimes);
            Console.ReadLine();
        }
    }
}

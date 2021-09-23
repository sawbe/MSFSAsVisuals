using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using MSFSAsVisuals.Shared;

namespace MSFSAsVisuals.Client
{
    class Program
    {
        static readonly string[] splitEndings = new string[] { Environment.NewLine };
        static SimConnector msfs;
        static TcpClient client;
        static string serverIp = "127.0.0.1";
        static void Main(string[] args)
        {
            if (args.Length > 0)
                serverIp = args[0];

            msfs = new SimConnector(5);
            Console.WriteLine("Connected to MSFS");

            CancellationTokenSource cts = new CancellationTokenSource();
            client = new TcpClient();

            Task con = new Task(() => ConnectToServer(cts.Token), TaskCreationOptions.LongRunning);
            con.Start();
            Console.ReadKey();
            Console.WriteLine("Exiting");
            cts.Cancel();
            con.Wait();
        }

        static async void ConnectToServer(CancellationToken token)
        {
            using (token.Register(client.Close))
            {
                try
                {
                    await client.ConnectAsync(serverIp, 7474);
                    Console.WriteLine("Connected to Server");
                    var stream = client.GetStream();
                    var buffer = new byte[8196];
                    int len = -1;
                    while (len != 0 && !token.IsCancellationRequested)
                    {
                        try
                        {
                            len = await stream.ReadAsync(buffer, 0, buffer.Length);

                            if (len > 0)
                            {
                                var trimmed = new byte[len];
                                Buffer.BlockCopy(buffer, 0, trimmed, 0, len);
                                ProcessPackets(trimmed);
                            }
                        }
                        catch(Exception ex) { Console.WriteLine(ex.Message); }
                    }
                    client.Close();
                }
                catch { }
            }                
        }

        static void ProcessPackets(byte[] data)
        {
            string jsonData = Encoding.ASCII.GetString(data);
            var jsonPackets = jsonData.Split(splitEndings, StringSplitOptions.RemoveEmptyEntries);

            foreach (var jsonPacket in jsonPackets)
            {
                string packetType = jsonPacket.Substring(0, 2);
                string actualPacket = jsonPacket.Substring(2);
                switch(packetType)
                {
                    case "P:":
                        SimObjectData simData = JsonConvert.DeserializeObject<SimObjectData>(actualPacket);
                        msfs.ProcessSimData(SimObjectDataRequest.Plane, simData);
                        break;
                    case "A:":
                        var accel = JsonConvert.DeserializeObject<SimObjectAccelerationData>(actualPacket);
                        msfs.ProcessSimData(SimObjectDataRequest.Accelerations, accel);
                        break;
                    case "V:":
                        var vel = JsonConvert.DeserializeObject<SimObjectVelocityData>(actualPacket);
                        msfs.ProcessSimData(SimObjectDataRequest.Velocities, vel);
                        break;
                }
            }
        }
    }
}

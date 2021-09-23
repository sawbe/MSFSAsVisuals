using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSAsVisuals.Server
{
    class Program
    {
        static BlockingCollection<string> packetsToSend;
        static TcpListener tcpListener;
        static NetworkStream tcpStream;
        static void Main(string[] args)
        {
            packetsToSend = new BlockingCollection<string>();
            CancellationTokenSource cts = new CancellationTokenSource();
            tcpListener = new TcpListener(IPAddress.Any, 7474);

            Task listen = new Task(() => StartServer(cts.Token), TaskCreationOptions.LongRunning);
            listen.Start();

            Task send = new Task(()=>DoSending(cts.Token), TaskCreationOptions.LongRunning);
            send.Start();

            Console.WriteLine("Server running");

            SimConnector p3d = new SimConnector(0, packetsToSend);
            Console.WriteLine("P3D Connected");

            Console.ReadKey();
            Console.WriteLine("Exiting");
            cts.Cancel();
            listen.Wait();
        }

        static async void StartServer(CancellationToken token)
        {
            using (token.Register(tcpListener.Stop))
            {
                tcpListener.Start();
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var client = await tcpListener.AcceptTcpClientAsync();
                        while (packetsToSend.Count > 0) { packetsToSend.TryTake(out _); }//clear the send queue

                        Console.WriteLine("Client connected");
                        tcpStream = client.GetStream();

                        byte[] buffer = new byte[8196];
                        int len = -1;
                        while(len != -1 && !token.IsCancellationRequested)
                        {
                            len = await tcpStream.ReadAsync(buffer, 0, buffer.Length);
                            //Process Read Data here
                        }
                    }
                    catch { }
                }
            }
        }

        static async void DoSending(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                string json;
                try
                {
                    packetsToSend.TryTake(out json, -1, token);
                    if (tcpStream != null && tcpStream.CanWrite)
                    {
                        json += Environment.NewLine;
                        byte[] buffer = Encoding.ASCII.GetBytes(json);
                        await tcpStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                catch { }
            }
        }
    }
}

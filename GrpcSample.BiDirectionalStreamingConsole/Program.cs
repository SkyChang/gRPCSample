using Duplex;
using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcSample.BiDirectionalStreamingConsole
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var name = "BiDirectionalStreaming-Debug";
            //if (args.Length != 1)
            //{
            //    Console.WriteLine("No name provided. Using <BiDirectionalStreaming>");
            //    name = args[0];
            //}

            ///
            /// Token init
            /// 
            //HttpClient httpClient = new HttpClient();
            //ApiService apiService = new ApiService(httpClient);
            //var token = await apiService.GetAccessTokenAsync();
            //var token = "This is invalid, I hope it fails";

            //var tokenValue = "Bearer " + token;
            //var metadata = new Metadata
            //{
            //    { "Authorization", tokenValue }
            //};

            ///
            /// Call gRPC HTTPS
            ///
            //var channelCredentials = new SslCredentials(
            //    File.ReadAllText("Certs\\ca.crt"),
            //        new KeyCertificatePair(
            //            File.ReadAllText("Certs\\client.crt"),
            //            File.ReadAllText("Certs\\client.key")
            //        )
            //    );

            var port = "50051";

            var channel = new Channel("localhost:" + port, ChannelCredentials.Insecure);//, channelCredentials);
            var client = new Messaging.MessagingClient(channel);

            using (var duplex = client.SendData())
            {
                Console.WriteLine($"Connected as {name}. Send empty message to quit.");

                // Dispatch, this could be racy
                var responseTask = Task.Run(async () =>
                {
                    while (await duplex.ResponseStream.MoveNext(CancellationToken.None))
                    {
                        Console.WriteLine($"{duplex.ResponseStream.Current.Name}: {duplex.ResponseStream.Current.Message}");
                    }
                });

                var line = Console.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    await duplex.RequestStream.WriteAsync(new MyMessage { Name = name, Message = line });
                    line = Console.ReadLine();
                }
                await duplex.RequestStream.CompleteAsync();
            }

            Console.WriteLine("Shutting down");
            await channel.ShutdownAsync();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            return 0;
        }
    }
}

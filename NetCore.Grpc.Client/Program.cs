using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using NetCore.Grpc.Common.Helpers;
using NetCore.Grpc.Common.Protos;

namespace NetCore.Grpc.Client
{
    class Program
    {
        private const string ConnectionString = "https://localhost:5001";

        private static readonly CancellationTokenSource CancellationTokenSource =
            new CancellationTokenSource();
        
        private static string _login;
        private static string _token;
        
        static async Task Main(string[] args)
        {
            try
            {
                // unable insecure connection 
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                using var channel = CreateAuthenticatedChannel();
                Console.Write("Enter login: ");
                var login = Console.ReadLine();
                Console.Write("Enter password: ");
                var password = Console.ReadLine();
                await AuthenticateAsync(login, password, channel);
                var cancellationToken = CancellationTokenSource.Token;
                await Task.WhenAll(SendMessageAsync(channel), SubscribeToNotificationsAsync(channel, cancellationToken));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        private static async Task AuthenticateAsync(string login, string password, ChannelBase channel)
        {
            try
            {
                var authService =
                    new AuthenticationService.AuthenticationServiceClient(channel);

                var result = await authService.LoginAsync(new AuthenticateRequest
                {
                    Login = login,
                    Password = password
                });

                _login = result.Login;
                _token = result.Token;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }
        
        private static GrpcChannel CreateAuthenticatedChannel()
        {
            var sslCredentials = new SslCredentials();
            var asyncAuthInterceptor = GetAsyncAuthInterceptor();
            
            var channelCredentials = ChannelCredentials.Create(
                sslCredentials, CallCredentials.FromInterceptor(asyncAuthInterceptor));
            
            return GrpcChannel.ForAddress(ConnectionString, new GrpcChannelOptions()
            {
                Credentials = channelCredentials
            });
        }

        private static AsyncAuthInterceptor GetAsyncAuthInterceptor()
        {
            var asyncAuthInterceptor = new AsyncAuthInterceptor((context, metadata) =>
            {
                if (string.IsNullOrEmpty(_token))
                {
                    return Task.CompletedTask;
                }

                metadata.Add(GrpcHelper.HeaderKey.Login, _login);
                metadata.Add(GrpcHelper.HeaderKey.Token, $"Bearer {_token}");

                return Task.CompletedTask;
            });
            return asyncAuthInterceptor;
        }

        private static async Task SendMessageAsync(ChannelBase channel)
        {
            try
            {
                var chatService = new ChatService.ChatServiceClient(channel);
                var messageContent = string.Empty;
                while (messageContent != "exit")
                {
                    Console.WriteLine("Enter message: ");
                    messageContent = Console.ReadLine();

                    var message = new Message
                    {
                        Login = _login,
                        Time = DateTime.UtcNow.ToTimestamp(),
                        Content = messageContent ?? string.Empty
                    };
                
                    await chatService.SendAsync(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
            
            CancellationTokenSource.Cancel();
        }

        private static async Task SubscribeToNotificationsAsync(ChannelBase channel, CancellationToken stoppingToken)
        {   
            while (!stoppingToken.IsCancellationRequested)
            {
                var notificationClient = new NotificationService.NotificationServiceClient(channel);
                using var call = notificationClient.Notify(new Empty());
                    
                try
                {
                    while (call?.ResponseStream != null && 
                           await (call.ResponseStream.MoveNext(stoppingToken) ?? Task.FromResult(false)))
                    {
                        var eventMessage = call.ResponseStream.Current;

                        switch (eventMessage.Type)
                        {
                            case EventType.NewMessage:
                                var message = Message.Parser.ParseFrom(eventMessage.Data);
                                if (message.Login == _login)
                                {
                                    return;
                                }
                                
                                var left = Console.CursorLeft;
                                var top = Console.CursorTop;
                                
                                Console.MoveBufferArea(0, top-1, Math.Max(left, 14), 2, 0, top + 2);
                                Console.SetCursorPosition(0, top-1);
                                Console.WriteLine($"\r\n{message.Login}({message.Time.ToDateTime():g}): {message.Content}");
                                Console.SetCursorPosition(left, top + 3);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Error.Write(e.Message);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using NetCore.Grpc.Common.Extensions;
using NetCore.Grpc.Common.Observer;
using NetCore.Grpc.Server.Models;
using proto = NetCore.Grpc.Common.Protos;

namespace NetCore.Grpc.Server.Services
{
    /// <summary>
    /// Proto NotificationService implementation
    /// </summary>
    [Authorize]
    public class NotificationService: proto.NotificationService.NotificationServiceBase
    {
        private readonly IGrpcObservable<proto.EventMessage> _grpcObservable;

        public NotificationService(IGrpcObservable<proto.EventMessage> grpcObservable)
        {
            _grpcObservable = grpcObservable;
        }

        public override async Task Notify(Empty request, IServerStreamWriter<proto.EventMessage> responseStream,
            ServerCallContext context)
        {
            var contextLogin = context.GetLogin();
            var subscriber = new GrpcNotificationObserver(contextLogin, responseStream);
            _grpcObservable.Register(subscriber);
         
            while (!_grpcObservable.HasUserUnsubscribed(subscriber.Login) &&
                   !context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(10000);
            }

            if (!context.CancellationToken.IsCancellationRequested)
            {
                _grpcObservable.Unregister(subscriber.Login);
            }
        }

        public override Task<Empty> Unsubscribe(Empty request, ServerCallContext context)
        {
            var contextLogin = context.GetLogin();
            _grpcObservable.Unregister(contextLogin);
            
            return Task.FromResult(new Empty());
        }
    }

}
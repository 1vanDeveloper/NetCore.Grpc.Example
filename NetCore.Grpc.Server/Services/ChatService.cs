using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using NetCore.Grpc.Common.Observer;
using proto = NetCore.Grpc.Common.Protos;

namespace NetCore.Grpc.Server.Services
{
    [Authorize]
    public class ChatService: proto.ChatService.ChatServiceBase
    {
        private readonly IGrpcObservable<proto.EventMessage> _grpcObservable;
        
        public ChatService(IGrpcObservable<proto.EventMessage> grpcObservable)
        {
            _grpcObservable = grpcObservable;
        }
        
        public override async Task<proto.Response> Send(proto.Message request, ServerCallContext context)
        {
            try
            {
                await _grpcObservable.NotifyObserversAsync(new proto.EventMessage
                { 
                    Type = proto.EventType.NewMessage,
                    Data = request.ToByteString()
                });
            }
            catch (Exception e)
            {
                return new proto.Response
                {
                    IsSuccess = false,
                    Description = e.Message
                };
            }
            
            return new proto.Response
            {
                IsSuccess = true
            };
        }
    }
}
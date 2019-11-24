using System.Threading.Tasks;
using Grpc.Core;
using NetCore.Grpc.Common.Observer;
using NetCore.Grpc.Common.Protos;

namespace NetCore.Grpc.Server.Models
{
    /// <summary>
    /// Subscriber for notifying
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class GrpcNotificationObserver : IGrpcObserver<EventMessage> 
    {
        public GrpcNotificationObserver(string name, IServerStreamWriter<EventMessage> responseStream)
        {
            Login = name;
            ResponseStream = responseStream;
        }

        /// <inheritdoc />
        public string Login { get; }

        /// <inheritdoc />
        public Task UpdateAsync(EventMessage obj)
        {
            return ResponseStream.WriteAsync(obj);
        }

        /// <inheritdoc />
        public bool HasUnsubscribed { get; set; }

        /// <inheritdoc />
        public IServerStreamWriter<EventMessage> ResponseStream { get; }}

}
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCore.Grpc.Common.Observer;
using proto = NetCore.Grpc.Common.Protos;

namespace NetCore.Grpc.Server.Services
{
    /// <summary>
    /// Service of notifying subscribers
    /// </summary>
    /// <typeparam name="T"> Data type </typeparam>
    public class GrpcNotificationObservable: IGrpcObservable<proto.EventMessage>
    {
        /// <summary>
        /// User dictionary
        /// </summary>
        private static readonly ConcurrentDictionary<string, IGrpcObserver<proto.EventMessage>> Observers =
            new ConcurrentDictionary<string, IGrpcObserver<proto.EventMessage>>();

        public GrpcNotificationObservable()
        {
            
        }

        /// <inheritdoc />
        public Task NotifyObserversAsync(proto.EventMessage obj)
        {
            var tasks = Observers
                .Where(x => !x.Value.HasUnsubscribed)
                .Select(observer => UpdateInnerAsync(observer, obj)).ToList();

            return Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public Task NotifyObserverAsync(proto.EventMessage obj, string observerLogin)
        {
            var tasks = Observers
                .Where(x => !x.Value.HasUnsubscribed && x.Key == observerLogin)
                .Select(observer => UpdateInnerAsync(observer, obj)).ToList();

            return Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public bool HasUserUnsubscribed(string observerLogin)
        {
            var result = Observers.TryGetValue(observerLogin, out var observer);

            return !result || observer.HasUnsubscribed;
        }

        /// <inheritdoc />
        public void Register(IGrpcObserver<proto.EventMessage> observer)
        {
            Observers.AddOrUpdate(observer.Login, observer, (s, grpcObserver) => observer);
        }

        /// <inheritdoc />
        public void Unregister(string observerLogin)
        {
            Observers.TryRemove(observerLogin, out _);
        }

        private async Task UpdateInnerAsync(KeyValuePair<string, IGrpcObserver<proto.EventMessage>> observer,
            proto.EventMessage obj)
        {
            await observer.Value.UpdateAsync(obj);
        }
    }
}
using System.Threading.Tasks;

namespace NetCore.Grpc.Common.Observer
{
    /// <summary>
    /// Notifier of subscribers.
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public interface IGrpcObservable<T>
    {
        /// <summary>
        /// Is user subscribed
        /// </summary>
        /// <param name="observerLogin"></param>
        /// <returns></returns>
        bool HasUserUnsubscribed(string observerLogin);

        /// <summary>
        /// Reg subscriber
        /// </summary>
        /// <param name="observer"></param>
        void Register(IGrpcObserver<T> observer);

        /// <summary>
        /// Notify subscribers
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task NotifyObserversAsync(T obj);

        /// <summary>
        /// Notify subscriber
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="observerLogin"></param>
        /// <returns></returns>
        Task NotifyObserverAsync(T obj, string observerLogin);

        /// <summary>
        /// Usubscribe user 
        /// </summary>
        /// <param name="observerLogin"></param>
        void Unregister(string observerLogin);
    }
}
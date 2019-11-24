using System.Threading.Tasks;
using Grpc.Core;

namespace NetCore.Grpc.Common.Observer
{
    /// <summary>
    /// Subscriber of notify service
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public interface IGrpcObserver<T>
    {
        /// <summary>
        /// User login
        /// </summary>
        string Login { get; }

        /// <summary>
        /// Is user subscribed
        /// </summary>
        bool HasUnsubscribed { get; set; }

        /// <summary>
        /// Notifying stream 
        /// </summary>
        IServerStreamWriter<T> ResponseStream { get; }

        /// <summary>
        /// Notify message
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task UpdateAsync(T obj);
    }
}
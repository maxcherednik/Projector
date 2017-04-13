
namespace Projector.Data
{
    /// <summary>
    /// Represents data provider
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Adds consumer to the list of data updates subscribers
        /// </summary>
        /// <param name="consumer">Instance of the IDataConsumer</param>
        /// <returns>Instance of IDisconnectable, which implements IDisposable. Used for consumer unsubscription</returns>
        IDisconnectable AddConsumer(IDataConsumer consumer);

        /// <summary>
        /// Removes consumer from the list of data updates subscribers
        /// </summary>
        /// <param name="consumer">Instance of the IDataConsumer</param>
        void RemoveConsumer(IDataConsumer consumer);
    }

    /// <summary>
    /// Generic representation of the IDataProvider.
    /// Used for type inference
    /// </summary>
    public interface IDataProvider<T> : IDataProvider
    {

    }
}

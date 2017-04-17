using System.Collections.Generic;

namespace Projector.Data
{
    /// <summary>
    /// Represents data consumer
    /// </summary>
    public interface IDataConsumer
    {
        /// <summary>
        /// This method should be called once during subscription
        /// </summary>
        /// <param name="schema">Instance of the schema, which represents data of the IDataProvider</param>
        void OnSchema(ISchema schema);

        /// <summary>
        /// This method called when data provider wants to notify that some rows were added
        /// </summary>
        /// <param name="rowIds">List of row ids which were added to the data provider</param>
        void OnAdd(IReadOnlyCollection<int> rowIds);

        /// <summary>
        /// This method called when data provider wants to notify that some rows were updated
        /// </summary>
        /// <param name="rowIds">List of row ids which were updated on the data provider</param>
        /// <param name="updatedFields">List of updated fields</param>
        void OnUpdate(IReadOnlyCollection<int> rowIds, IReadOnlyCollection<IField> updatedFields);

        /// <summary>
        /// This method called when data provider wants to notify that some rows were deleted
        /// </summary>
        /// <param name="rowIds">List of row ids which were deleted from the data provider</param>
        void OnDelete(IReadOnlyCollection<int> rowIds);

        /// <summary>
        /// This method is used as a signal of batch processing
        /// </summary>
        void OnSyncPoint();
    }
}

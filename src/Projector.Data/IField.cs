using System;

namespace Projector.Data
{
    /// <summary>
    /// Represents read-only information about column of the tabular data
    /// </summary>
    public interface IField
    {
        /// <summary>
        /// Represents type of the data stored in the column
        /// </summary>
        /// <returns>Type of the value</returns>
        Type DataType { get; }

        /// <summary>
        /// Represents name of the column
        /// </summary>
        /// <returns>Returns name of the column</returns>
        string Name { get; }
    }

    /// <summary>
    /// Represents read-only field of the table of arbitrary type 
    /// </summary>
    public interface IField<TData> : IField
    {
        /// <summary>
        /// Returns value from the specified row
        /// </summary>
        /// <param name="rowId">Position of the requested value in the column</param>
        /// <returns>Returns value</returns>
        TData GetValue(int rowId);
    }
}

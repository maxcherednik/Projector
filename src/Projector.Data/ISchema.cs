using System.Collections.Generic;

namespace Projector.Data
{
    /// <summary>
    /// Represents read-only schema of the table.
    /// Using schema <see cref="IField{T}" /> instance can be retrieved for reading data
    /// </summary>
    public interface ISchema
    {
        /// <summary>
        /// Returns read-only list of <see cref="IField" /> objects which decribes table schema
        /// </summary>
        IReadOnlyList<IField> Columns { get; }

        /// <summary>
        /// Returns instance of <see cref="IField{T}" /> from which value of the row could be retrieved
        /// </summary>
        /// <param name="name">Name of the column</param>
        /// <returns>Returns instance of <see cref="IField{T}" /> </returns>
        IField<T> GetField<T>(string name);
    }
}

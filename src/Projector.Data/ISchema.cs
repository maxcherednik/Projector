using System.Collections.Generic;

namespace Projector.Data
{
    public interface ISchema
    {
        IReadOnlyList<IField> Columns { get; }

        IField<T> GetField<T>(int id, string name);
    }
}

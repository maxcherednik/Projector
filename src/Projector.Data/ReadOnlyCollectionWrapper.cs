using System.Collections;
using System.Collections.Generic;

namespace Projector.Data
{
    class ReadOnlyCollectionWrapper<T> : IReadOnlyCollection<T>
    {
        private readonly ICollection<T> _sourceCollection;

        public ReadOnlyCollectionWrapper(ICollection<T> sourceCollection)
        {
            _sourceCollection = sourceCollection;
        }

        public int Count => _sourceCollection.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return _sourceCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _sourceCollection.GetEnumerator();
        }
    }
}

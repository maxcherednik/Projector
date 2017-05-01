using System;
using System.Collections.Generic;

namespace Projector.Data.Join
{
    public interface IKeyFieldMatcher
    {
        bool Match(ISchema leftSchema, int leftRowId, ISchema rightSchema, int rightRowId);
    }

    public class KeyFieldMatcher<TKey> : IKeyFieldMatcher
    {
        private Func<ISchema, int, TKey> _leftKeyAccessor;
        private Func<ISchema, int, TKey> _rightKeyAccessor;

        public KeyFieldMatcher(Func<ISchema, int, TKey> leftKeyAccessor, Func<ISchema, int, TKey> rightKeyAccessor)
        {
            _leftKeyAccessor = leftKeyAccessor;
            _rightKeyAccessor = rightKeyAccessor;
        }

        public bool Match(ISchema leftSchema, int leftRowId, ISchema rightSchema, int rightRowId)
        {
            var leftKeyValue = _leftKeyAccessor(leftSchema, leftRowId);
            var rightKeyValue = _rightKeyAccessor(rightSchema, rightRowId);

            return EqualityComparer<TKey>.Default.Equals(leftKeyValue, rightKeyValue);
        }
    }
}

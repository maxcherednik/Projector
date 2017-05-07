using System;
using System.Collections.Generic;

namespace Projector.Data.Join
{
    public class KeyFieldsMeta
    {
        public KeyFieldsMeta(ISet<string> leftKeyFieldNames,
                             ISet<string> rightKeyFieldNames,
                             Func<ISchema, int, ISchema, int, bool> rowMatcher)
        {
            LeftKeyFieldNames = leftKeyFieldNames;
            RightKeyFieldNames = rightKeyFieldNames;
            RowMatcher = rowMatcher;
        }

        public ISet<string> LeftKeyFieldNames { get; }

        public ISet<string> RightKeyFieldNames { get; }

        public Func<ISchema, int, ISchema, int, bool> RowMatcher { get; }
    }
}

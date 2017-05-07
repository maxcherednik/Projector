using System.Collections.Generic;

namespace Projector.Data.Join
{
    public class JoinProjectedFieldsMeta
    {
        public JoinProjectedFieldsMeta(IDictionary<string, ISet<string>> leftSourceOldNamesToNewNamesMapping,
            IDictionary<string, ISet<string>> rightSourceOldNamesToNewNamesMapping,
            IDictionary<string, IField> projectedFields)
        {
            LeftSourceOldNamesToNewNamesMapping = leftSourceOldNamesToNewNamesMapping;
            RightSourceOldNamesToNewNamesMapping = rightSourceOldNamesToNewNamesMapping;
            ProjectedFields = projectedFields;
        }

        public IDictionary<string, ISet<string>> LeftSourceOldNamesToNewNamesMapping { get; }

        public IDictionary<string, ISet<string>> RightSourceOldNamesToNewNamesMapping { get; }

        public IDictionary<string, IField> ProjectedFields { get; }
    }
}


using System.Collections.Generic;
namespace Projector.Data
{
    public interface IDataConsumer
    {
        void OnAdd(IList<int> ids);
        void OnUpdate(IList<int> ids, IList<IField> updatedFields);
        void OnDelete(IList<int> ids);
        void OnSchema(ISchema schema);
        void OnSyncPoint();
    }
}

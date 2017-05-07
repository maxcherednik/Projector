
namespace Projector.Data.Tables
{
    public interface IWritableField : IField
    {
        void EnsureCapacity(int rowId);

        void CleanOldValue(int rowId);
    }

    public interface IWritableField<TData> : IField<TData>, IWritableField
    {
        void SetValue(int rowId, TData value);
    }
}

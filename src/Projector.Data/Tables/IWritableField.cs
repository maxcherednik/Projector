
namespace Projector.Data.Tables
{
    public interface IWritableField : IField
    {
        void SetCurrentRow(int rowId);

        void EnsureCapacity(int rowId);
    }

    public interface IWritableField<TData> : IField<TData>, IWritableField
    {
        void SetValue(TData value);
    }
}

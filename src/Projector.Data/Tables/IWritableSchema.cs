namespace Projector.Data.Tables
{
    public interface IWritebleSchema : ISchema
    {
        IWritableField<T> GetWritableField<T>(int id, string name);
        int GetNewRowId();
        void Remove(int rowIndex);
    }
}

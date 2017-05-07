namespace Projector.Data.Tables
{
    public interface IWritebleSchema : ISchema
    {
        IWritableField<T> GetWritableField<T>(string name);

        int GetNewRowId();

        void Remove(int rowIndex);
    }
}

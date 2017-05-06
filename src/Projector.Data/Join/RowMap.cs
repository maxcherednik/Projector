namespace Projector.Data.Join
{
    public struct RowMap
    {
        public RowMap(int leftRowId, int rightRowId)
        {
            LeftRowId = leftRowId;
            RightRowId = rightRowId;
        }

        public int LeftRowId { get; }

        public int RightRowId { get; }
    }
}

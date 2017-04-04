
namespace Projector.Data.GroupBy
{
    public class GroupBy<TSource,TDest> : GroupBy, IDataProvider<TDest>
    {
        public GroupBy(IDataProvider<TSource> source)
            : base(source)
        {

        }
    }
}

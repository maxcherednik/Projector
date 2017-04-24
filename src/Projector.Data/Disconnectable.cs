
namespace Projector.Data
{
    public class Disconnectable : IDisconnectable
    {
        private readonly IDataProvider _dataProvider;
        private readonly IDataConsumer _dataConsumer;

        public Disconnectable(IDataProvider dataProvider, IDataConsumer dataConsumer)
        {
            _dataProvider = dataProvider;
            _dataConsumer = dataConsumer;
        }

        public void Dispose()
        {
            _dataProvider.RemoveConsumer(_dataConsumer);
        }
    }
}

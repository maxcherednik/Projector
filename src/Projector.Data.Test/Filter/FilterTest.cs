using NSubstitute;
using NUnit.Framework;
using Projector.Data.Filter;
using Projector.Data.Test.Helpers;
using System.Collections.Generic;

namespace Projector.Data.Test.Filter
{
    [TestFixture]
    class FilterTest
    {
        private Filter<Client> _filter;
        private IDataProvider<Client> _dataProvider;
        private IDisconnectable _dataProviderUnsubscriber;
        private ISchema _dataProviderSchema;
        private List<int> _ids;

        private IDataConsumer _filterConsumer;

        [SetUp]
        public void InitContext()
        {
            _ids = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            _dataProviderSchema = Substitute.For<ISchema>();

            _dataProvider = Substitute.For<IDataProvider<Client>>();

            _dataProviderUnsubscriber = Substitute.For<IDisconnectable>();
            _dataProvider.AddConsumer(Arg.Any<IDataConsumer>()).Returns(_dataProviderUnsubscriber);
            _dataProvider.AddConsumer(Arg.Do<IDataConsumer>(x =>
            {
                x.OnSchema(_dataProviderSchema);
                x.OnAdd(_ids);
                x.OnSyncPoint();
            }));


            _filterConsumer = Substitute.For<IDataConsumer>();

            _filter = new Filter<Client>(_dataProvider, x => true);
        }

        [Test]
        public void InitFilterTest()
        {
            _dataProvider.Received(1).AddConsumer(_filter);
        }


        [Test]
        public void ChangeFilterTest()
        {
            _dataProvider.ClearReceivedCalls();
            _filter.ChangeFilter(x => true);

            _dataProviderUnsubscriber.Received(1).Dispose();
            _dataProvider.Received(1).AddConsumer(_filter);
        }

        [Test]
        public void FilterAsProviderTest()
        {
            // set up
            var filteredIds = new List<int>();
            _filterConsumer.OnAdd(Arg.Do<IList<int>>(ids => filteredIds.AddRange(ids)));

            // call
            _filter.AddConsumer(_filterConsumer);

            // check
            _filterConsumer.Received(1).OnSchema(_dataProviderSchema);
            Assert.AreEqual(10, filteredIds.Count);


            _dataProvider.ClearReceivedCalls();
            // set up
            _filterConsumer.OnDelete(Arg.Do<IList<int>>(ids =>
            {
                foreach (var id in ids)
                {
                    filteredIds.Remove(id);
                }
            }));

            // call
            _filter.ChangeFilter(x => false);

            // check
            _dataProviderUnsubscriber.Received(1).Dispose();
            _dataProvider.Received(1).AddConsumer(_filter);

            Assert.AreEqual(0, filteredIds.Count);
        }


    }
}

using NSubstitute;
using Projector.Data.Filter;
using Projector.Data.Test.Helpers;
using System.Collections.Generic;
using Xunit;

namespace Projector.Data.Test.Filter
{
    class FilterTest
    {
        private readonly Filter<Client> _filter;
        private readonly IDataProvider<Client> _dataProvider;
        private readonly IDisconnectable _dataProviderUnsubscriber;
        private readonly ISchema _dataProviderSchema;
        private readonly List<int> _ids;

        private readonly IDataConsumer _filterConsumer;

        public FilterTest()
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

        [Fact]
        public void InitFilterTest()
        {
            var dataProvider = Substitute.For<IDataProvider<Client>>();

            var filterConsumer = Substitute.For<IDataConsumer>();

            var filter = new Filter<Client>(_dataProvider, x => true);

            dataProvider.Received(1).AddConsumer(filter);
        }

        [Fact]
        public void WhenFilterMatches()
        {
            var dataProvider = Substitute.For<IDataProvider<Client>>();

            var filterConsumer = Substitute.For<IDataConsumer>();

            var filter = new Filter<Client>(_dataProvider, x => x.Id > 2);

            var field =Substitute.For<IField>();

            var dataProviderSchema = Substitute.For<ISchema>();

            dataProviderSchema.GetField<int>("Id").Returns(field);
            
            filter.OnSchema(dataProviderSchema);

            filter.OnAdd(new List<int> { 1, 2 });

        }

        [Fact]
        public void ChangeFilterTest()
        {
            _dataProvider.ClearReceivedCalls();
            _filter.ChangeFilter(x => true);

            _dataProviderUnsubscriber.Received(1).Dispose();
            _dataProvider.Received(1).AddConsumer(_filter);
        }

        [Fact]
        public void FilterAsProviderTest()
        {
            // set up
            var filteredIds = new List<int>();
            _filterConsumer.OnAdd(Arg.Do<IReadOnlyCollection<int>>(filteredIds.AddRange));

            // call
            _filter.AddConsumer(_filterConsumer);

            // check
            _filterConsumer.Received(1).OnSchema(_dataProviderSchema);
            Assert.Equal(10, filteredIds.Count);


            _dataProvider.ClearReceivedCalls();
            // set up
            _filterConsumer.OnDelete(Arg.Do<IReadOnlyCollection<int>>(ids =>
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

            Assert.Equal(0, filteredIds.Count);
        }
    }
}

using NSubstitute;
using Projector.Data.Filter;
using Projector.Data.Test.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Projector.Data.Test.Filter
{
    public class FilterTest
    {
        [Fact]
        public void InitFilterTest()
        {
            var dataProvider = Substitute.For<IDataProvider<Client>>();

            var filter = new Filter<Client>(dataProvider, x => true);

            dataProvider.Received(1).AddConsumer(filter);
        }

        [Fact]
        public void WhenFilterMatches()
        {
            var dataProvider = Substitute.For<IDataProvider<Client>>();

            var filter = new Filter<Client>(dataProvider, x => x.Id > 2);

            var field = Substitute.For<IField<int>>();

            var dataProviderSchema = Substitute.For<ISchema>();

            dataProviderSchema.GetField<int>("Id").Returns(field);

            filter.Received(1).OnSchema(dataProviderSchema);

            filter.Received(1).OnAdd(new List<int> { 1, 2 });
        }

        [Fact]
        public void ChangeFilterTest()
        {
            var dataProvider = Substitute.For<IDataProvider<Client>>();

            var filter = new Filter<Client>(dataProvider, x => true);

            filter.ChangeFilter(x => true);
            
            dataProvider.Received(1).AddConsumer(filter);
        }

        [Fact]
        public void FilterAsProviderTest()
        {
            // set up

            var initialIds = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var dataProvider = Substitute.For<IDataProvider<Client>>();

            var filter = new Filter<Client>(dataProvider, x => true);

            var filteredIds = new List<int>();

            var filterConsumer = Substitute.For<IDataConsumer>();

            filterConsumer.OnAdd(Arg.Do<IReadOnlyCollection<int>>(filteredIds.AddRange));

            var dataProviderSchema = Substitute.For<ISchema>();

            var dataProviderUnsubscriber = Substitute.For<IDisconnectable>();
            dataProvider.AddConsumer(Arg.Any<IDataConsumer>()).Returns(dataProviderUnsubscriber);
            dataProvider.AddConsumer(Arg.Do<IDataConsumer>(x =>
            {
                x.OnSchema(dataProviderSchema);
                x.OnAdd(initialIds);
                x.OnSyncPoint();
            }));



            // call
            filter.AddConsumer(filterConsumer);

            // check
            filterConsumer.Received(1).OnSchema(dataProviderSchema);
            Assert.Equal(10, filteredIds.Count);


            dataProvider.ClearReceivedCalls();
            // set up
            filterConsumer.OnDelete(Arg.Do<IReadOnlyCollection<int>>(ids =>
            {
                foreach (var id in ids)
                {
                    filteredIds.Remove(id);
                }
            }));

            // call
            filter.ChangeFilter(x => false);

            // check
            dataProviderUnsubscriber.Received(1).Dispose();
            dataProvider.Received(1).AddConsumer(filter);

            Assert.Equal(0, filteredIds.Count);
        }
    }
}

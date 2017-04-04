using NSubstitute;
using NUnit.Framework;
using Projector.Data.Tables;
using System.Collections.Generic;
using System.Linq;

namespace Projector.Data.Test.Tables
{
    [TestFixture]
    public class TableTest
    {
        private IDataConsumer _mockDataConsumer;
        private IWritebleSchema _mockSchema;

        private Table _table;

        [SetUp]
        public void InitContext()
        {
            _mockDataConsumer = Substitute.For<IDataConsumer>();
            _mockSchema = Substitute.For<IWritebleSchema>();
            _table = new Table(_mockSchema);
        }

        [Test]
        public void TestSubscribeWhenZeroRowsInside()
        {
            _table.AddConsumer(_mockDataConsumer);

            Received.InOrder(() =>
                {
                    _mockDataConsumer.Received(1).OnSchema(_mockSchema);
                    _mockDataConsumer.Received(1).OnSyncPoint();
                });

            _mockDataConsumer.DidNotReceive().OnAdd(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnUpdate(Arg.Any<IList<int>>(), Arg.Any<IList<IField>>());
            _mockDataConsumer.DidNotReceive().OnDelete(Arg.Any<IList<int>>());
        }

        [Test]
        public void TestSubscribeWhenSeveralRowsInside()
        {
            var idsToAdd = new List<int>(){ 0, 1, 2 };
            _mockSchema.GetNewRowId().Returns(0, 1, 2);
            
            _table.NewRow();
            _table.NewRow();
            _table.NewRow();

            _table.FireChanges();

            _mockDataConsumer.ClearReceivedCalls();


            _table.AddConsumer(_mockDataConsumer);

            Received.InOrder(() =>
                {
                    _mockDataConsumer.Received(1).OnSchema(_mockSchema);
                    _mockDataConsumer.Received(1).OnAdd(Arg.Is<IList<int>>(x => idsToAdd.SequenceEqual(x)));
                    _mockDataConsumer.Received(1).OnSyncPoint();
                });

            _mockDataConsumer.DidNotReceive().OnUpdate(Arg.Any<IList<int>>(), Arg.Any<IList<IField>>());
            _mockDataConsumer.DidNotReceive().OnDelete(Arg.Any<IList<int>>());
        }

        [Test]
        public void AddNewRowWithoutFireChangesTest()
        {
            _table.AddConsumer(_mockDataConsumer);

            _mockDataConsumer.ClearReceivedCalls();

            _table.NewRow();

            _mockDataConsumer.DidNotReceive().OnSchema(Arg.Any<ISchema>());
            _mockDataConsumer.DidNotReceive().OnAdd(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnUpdate(Arg.Any<IList<int>>(), Arg.Any<IList<IField>>());
            _mockDataConsumer.DidNotReceive().OnDelete(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnSyncPoint();
        }

        [Test]
        public void AddNewRowWithFireChangesTest()
        {
            var args = new List<int>();
            _table.AddConsumer(_mockDataConsumer);

            _mockDataConsumer.ClearReceivedCalls();

            _mockDataConsumer.OnAdd(Arg.Do<IList<int>>(list => args.AddRange(list)));

            _table.NewRow();

            _table.FireChanges();

            _mockDataConsumer.DidNotReceive().OnSchema(Arg.Any<ISchema>());
            _mockDataConsumer.Received(1).OnAdd(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnUpdate(Arg.Any<IList<int>>(), Arg.Any<IList<IField>>());
            _mockDataConsumer.DidNotReceive().OnDelete(Arg.Any<IList<int>>());
            _mockDataConsumer.Received(1).OnSyncPoint();

            Assert.AreEqual(1, args.Count);
            Assert.AreEqual(0, args[0]);

        }


        [Test]
        public void DeleteRowWithoutFireChangesTest()
        {
            _table.AddConsumer(_mockDataConsumer);

            _table.NewRow();

            _mockDataConsumer.ClearReceivedCalls();

            _table.RemoveRow(0);

            _mockDataConsumer.DidNotReceive().OnSchema(Arg.Any<ISchema>());
            _mockDataConsumer.DidNotReceive().OnAdd(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnUpdate(Arg.Any<IList<int>>(), Arg.Any<IList<IField>>());
            _mockDataConsumer.DidNotReceive().OnDelete(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnSyncPoint();
        }

        [Test]
        public void DeleteRowWithFireChangesTest()
        {
            var args = new List<int>();

            _table.AddConsumer(_mockDataConsumer);

            _table.NewRow();

            _table.FireChanges();

            _mockDataConsumer.ClearReceivedCalls();

            _mockDataConsumer.OnDelete(Arg.Do<IList<int>>(list => args.AddRange(list)));

            _table.RemoveRow(0);

            _table.FireChanges();

            _mockDataConsumer.DidNotReceive().OnSchema(Arg.Any<ISchema>());
            _mockDataConsumer.DidNotReceive().OnAdd(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnUpdate(Arg.Any<IList<int>>(), Arg.Any<IList<IField>>());
            _mockDataConsumer.Received(1).OnDelete(Arg.Any<IList<int>>());
            _mockDataConsumer.Received(1).OnSyncPoint();

            Assert.AreEqual(1, args.Count);
            Assert.AreEqual(0, args[0]);
        }


        [Test]
        public void FireChangesWithoutActualChangesTest()
        {
            _table.AddConsumer(_mockDataConsumer);

            _mockDataConsumer.ClearReceivedCalls();

            _table.FireChanges();

            _mockDataConsumer.DidNotReceive().OnSchema(Arg.Any<ISchema>());
            _mockDataConsumer.DidNotReceive().OnAdd(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnUpdate(Arg.Any<IList<int>>(), Arg.Any<IList<IField>>());
            _mockDataConsumer.DidNotReceive().OnDelete(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnSyncPoint();
        }

        [Test]
        public void FireChangesSeveralTimesTest()
        {
            _table.AddConsumer(_mockDataConsumer);

            _mockDataConsumer.ClearReceivedCalls();

            _table.NewRow();

            _table.FireChanges();

            _mockDataConsumer.ClearReceivedCalls();

            _table.FireChanges(); // we are testing this one

            _mockDataConsumer.DidNotReceive().OnSchema(Arg.Any<ISchema>());
            _mockDataConsumer.DidNotReceive().OnAdd(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnUpdate(Arg.Any<IList<int>>(), Arg.Any<IList<IField>>());
            _mockDataConsumer.DidNotReceive().OnDelete(Arg.Any<IList<int>>());
            _mockDataConsumer.DidNotReceive().OnSyncPoint();
        }
    }
}

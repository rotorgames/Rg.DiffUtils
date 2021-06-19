using System.Collections.Specialized;
using NUnit.Framework;

namespace Rg.DiffUtils.Tests
{
    [TestFixture]
    public class DiffObservableCollectionTest
    {
        [Test]
        public void AddRange()
        {
            var str = "abc";

            var c = new DiffObservableCollection<char>();

            NotifyCollectionChangedEventArgs ev = null;

            c.CollectionChanged += (sender, e) =>
            {
                ev = e;
            };

            c.AddRange(str);

            Assert.IsNotNull(ev);

            Assert.AreEqual(NotifyCollectionChangedAction.Add, ev.Action);
            Assert.AreEqual(0, ev.NewStartingIndex);
            CollectionAssert.AreEqual(str, ev.NewItems);
            CollectionAssert.AreEqual(str, c);
        }

        [Test]
        public void InsertRange()
        {
            var str = "bc";

            var c = new DiffObservableCollection<char>("ad");

            NotifyCollectionChangedEventArgs ev = null;

            c.CollectionChanged += (sender, e) =>
            {
                ev = e;
            };

            c.InsertRange(1, str);

            Assert.IsNotNull(ev);

            Assert.AreEqual(NotifyCollectionChangedAction.Add, ev.Action);
            Assert.AreEqual(1, ev.NewStartingIndex);
            CollectionAssert.AreEqual(str, ev.NewItems);
            CollectionAssert.AreEqual("abcd", c);
        }

        [Test]
        public void RemoveItems()
        {
            var str = "bc";

            var c = new DiffObservableCollection<char>("abcd");

            NotifyCollectionChangedEventArgs ev = null;

            c.CollectionChanged += (sender, e) =>
            {
                ev = e;
            };

            c.RemoveRange(str);

            Assert.IsNotNull(ev);

            Assert.AreEqual(NotifyCollectionChangedAction.Remove, ev.Action);
            Assert.AreEqual(-1, ev.OldStartingIndex);
            CollectionAssert.AreEqual(str, ev.OldItems);
            CollectionAssert.AreEqual("ad", c);
        }

        [Test]
        public void RemoveRange()
        {
            var c = new DiffObservableCollection<char>("abcd");

            NotifyCollectionChangedEventArgs ev = null;

            c.CollectionChanged += (sender, e) =>
            {
                ev = e;
            };

            c.RemoveRange(1, 2);

            Assert.IsNotNull(ev);

            Assert.AreEqual(NotifyCollectionChangedAction.Remove, ev.Action);
            Assert.AreEqual(1, ev.OldStartingIndex);
            CollectionAssert.AreEqual("bc", ev.OldItems);
            CollectionAssert.AreEqual("ad", c);
        }

        [Test]
        public void Move()
        {
            var c = new DiffObservableCollection<char>("abcd");

            NotifyCollectionChangedEventArgs ev = null;

            c.CollectionChanged += (sender, e) =>
            {
                ev = e;
            };

            c.Move(1, 2);

            Assert.IsNotNull(ev);

            Assert.AreEqual(NotifyCollectionChangedAction.Move, ev.Action);
            Assert.AreEqual(1, ev.OldStartingIndex);
            Assert.AreEqual(2, ev.NewStartingIndex);
            CollectionAssert.AreEqual("b", ev.OldItems);
            CollectionAssert.AreEqual("b", ev.NewItems);
            CollectionAssert.AreEqual("acbd", c);
        }
    }
}

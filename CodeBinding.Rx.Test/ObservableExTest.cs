using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeBinding.Test;
using System.Reactive.Concurrency;

namespace CodeBinding.Rx.Test
{
    [TestClass]
    public class ObservableExTest
    {
        [TestMethod]
        public void Create_SimpleExpression()
        {
            var source = new GenericNotifyPropertyChanged<int>();
            var expected = (new int[] {0, 1, 2}).ToObservable();

            var actual = ObservableEx.Create(() => source.Value);

            using (actual.SequenceEqual(expected)
                .ObserveOn(Scheduler.Immediate)
                .Subscribe(Assert.IsTrue, Assert.Fail))
            {
                source.Value = 1;
                source.Value = 2;
            }
        }

        [TestMethod]
        public void Create_ComplexExpression()
        {
            var source1 = new GenericNotifyPropertyChanged<int>();
            var source2 = new GenericNotifyPropertyChanged<int>();
            var expected = (new int[] { 0, 1, 2 }).ToObservable();

            var actual = ObservableEx.Create(() => source1.Value + source2.Value);

            bool completed = false;
            using (actual.Take(3).SequenceEqual(expected)
                .ObserveOn(Scheduler.Immediate)
                .Subscribe(Assert.IsTrue, e => Assert.Fail("Exception occured"), () => completed = true))
            {
                source1.Value = 1;
                source2.Value = 1;
            }

            Assert.IsTrue(completed, "Sequence not complete");
        }

        [TestMethod]
        public void Create_SimpleExpression_WrongInitialValue_CallsOnError()
        {
            var source = new GenericNotifyPropertyChanged<GenericNotifyPropertyChanged<int>>();

            var actual = ObservableEx.Create(() => source.Value.Value);

            bool completed = false;
            Exception error = null;
            using (actual.Subscribe(
                i => Assert.Fail("OnNext"),
                e => error = e, 
                () => completed = true))
            {
                source.Value = new GenericNotifyPropertyChanged<int>();
                source.Value.Value = 1;
            }

            Assert.IsNotNull(error, "OnError not called");
            Assert.IsInstanceOfType(error, typeof(NullReferenceException), "Wrong type of exception");
            Assert.IsFalse(completed, "Sequence complete");
        }

        [TestMethod]
        public void Create_SimpleExpression_WrongValueSet_CallsOnError()
        {
            var source = new GenericNotifyPropertyChanged<GenericNotifyPropertyChanged<int>>();
            source.Value = new GenericNotifyPropertyChanged<int>();
            var expected = (new int[] { 0, 1 }).ToObservable();
            // TODO: Add this to expected sequence after SequenceEqual is switched
            // to somethin more elaborate
            //.Concat<int>(Observable.Throw<int>(new NullReferenceException()));

            var actual = ObservableEx.Create(() => source.Value.Value);

            bool completed = false;
            Exception error = null;
            using (actual.SequenceEqual(expected)
                .ObserveOn(Scheduler.Immediate).Subscribe(
                Assert.IsTrue,
                e => error = e,
                () => completed = true))
            {
                source.Value.Value = 1;
                source.Value = null;
            }

            Assert.IsNotNull(error, "OnError not called");
            Assert.IsInstanceOfType(error, typeof(NullReferenceException), "Wrong type of exception");
            Assert.IsFalse(completed, "Sequence complete");
        }
    }
}

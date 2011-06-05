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
        public void FromExpression_SimpleExpression()
        {
            var source = new GenericNotifyPropertyChanged<int>();
            var expected = (new int[] {0, 1, 2}).ToObservable();

            var actual = ObservableEx.FromExpression(() => source.Value);

            using (actual.SequenceEqual(expected)
                .ObserveOn(Scheduler.Immediate)
                .Subscribe(Assert.IsTrue, Assert.Fail))
            {
                source.Value = 1;
                source.Value = 2;
            }
        }

        [TestMethod]
        public void FromExpression_ComplexExpression()
        {
            var source1 = new GenericNotifyPropertyChanged<int>();
            var source2 = new GenericNotifyPropertyChanged<int>();
            var expected = (new int[] { 0, 1, 2 }).ToObservable();

            var actual = ObservableEx.FromExpression(() => source1.Value + source2.Value);

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
    }
}

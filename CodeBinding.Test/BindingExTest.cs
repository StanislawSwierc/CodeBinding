using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Data;

namespace CodeBinding.Test
{
    [TestClass]
    public class BindingExTest
    {
        [TestMethod]
        public void FromExpression_SimpleBinding()
        {
            var target = new GenericBindingTarget<int>();
            var source = new GenericNotifyPropertyChanged<int>();
            
            BindingBase binding = BindingEx.FromExpression(() => source.Value);
            target.SetBinding(GenericBindingTarget<int>.ValueProperty, binding);
            
            Assert.AreEqual(0, target.Value);
            source.Value = 1;
            Assert.AreEqual(1, target.Value);
        }

        [TestMethod]
        public void FromExpression_ComplexBinding()
        {
            var target = new GenericBindingTarget<int>();
            var source1 = new GenericNotifyPropertyChanged<int>();
            var source2 = new GenericNotifyPropertyChanged<int>();

            BindingBase binding = BindingEx.FromExpression(() => source1.Value + source2.Value);
            target.SetBinding(GenericBindingTarget<int>.ValueProperty, binding);

            Assert.AreEqual(0, target.Value);
            source1.Value = 1;
            Assert.AreEqual(1, target.Value);
            source2.Value = 1;
            Assert.AreEqual(2, target.Value);
        }

        [TestMethod]
        public void BindFromExpression_SimpleBinding()
        {
            var target = new GenericBindingTarget<int>();
            var source = new GenericNotifyPropertyChanged<int>();
            
            BindingEx.BindFromExpression(() => target.Value == source.Value);
            
            Assert.AreEqual(0, target.Value);
            source.Value = 1;
            Assert.AreEqual(1, target.Value);
        }

        [TestMethod]
        public void BindFromExpression_ComplexBinding()
        {
            var target = new GenericBindingTarget<int>();
            var source1 = new GenericNotifyPropertyChanged<int>();
            var source2 = new GenericNotifyPropertyChanged<int>();

            BindingEx.BindFromExpression(() => target.Value == source1.Value + source2.Value);

            Assert.AreEqual(0, target.Value);
            source1.Value = 1;
            Assert.AreEqual(1, target.Value);
            source2.Value = 1;
            Assert.AreEqual(2, target.Value);
        }

        [TestMethod]
        public void BindFromExpression_SimpleBinding_DisposeSubsription_RemovesBinding()
        {
            var target = new GenericBindingTarget<int>();
            var source = new GenericNotifyPropertyChanged<int>();

            using (BindingEx.BindFromExpression(() => target.Value == source.Value))
            {
                Assert.AreEqual(0, target.Value);
                source.Value = 1;
                Assert.AreEqual(1, target.Value);
            }
            source.Value = 2;
            // Value should be reseted to 0
            Assert.AreEqual(0, target.Value);
        }

        [TestMethod]
        public void BindFromExpression_ComplexBinding_DisposeSubsription_RemovesBinding()
        {
            var target = new GenericBindingTarget<int>();
            var source1 = new GenericNotifyPropertyChanged<int>();
            var source2 = new GenericNotifyPropertyChanged<int>();

            using (BindingEx.BindFromExpression(() => target.Value == source1.Value + source2.Value))
            {
                Assert.AreEqual(0, target.Value);
                source1.Value = 1;
                Assert.AreEqual(1, target.Value);
                source2.Value = 1;
                Assert.AreEqual(2, target.Value);
            }
            source1.Value = 2;
            source2.Value = 2;
            // Value should be reseted to 0
            Assert.AreEqual(0, target.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void BindFromExpression_SimpleBinding_WrongInitialValue_ThrowsExeption()
        {
            var target = new GenericBindingTarget<int>();
            var source = new GenericNotifyPropertyChanged<GenericNotifyPropertyChanged<int>>();

            BindingEx.BindFromExpression(() => target.Value == source.Value.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void BindFromExpression_ComplexBinding_WrongInitialValue_ThrowsExeption()
        {
            var target = new GenericBindingTarget<int>();
            var source1 = new GenericNotifyPropertyChanged<int>();
            var source2 = new GenericNotifyPropertyChanged<GenericNotifyPropertyChanged<int>>();

            BindingEx.BindFromExpression(() => target.Value == source1.Value + source2.Value.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void BindFromExpression_SimpleBinding_WrongValueSet_ThrowsExeption()
        {
            var target = new GenericBindingTarget<int>();
            var source = new GenericNotifyPropertyChanged<GenericNotifyPropertyChanged<int>>();
            source.Value = new GenericNotifyPropertyChanged<int>();

            BindingEx.BindFromExpression(() => target.Value == source.Value.Value);

            Assert.AreEqual(0, target.Value);
            source.Value.Value = 1;
            Assert.AreEqual(1, target.Value);
            source.Value = null;
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void BindFromExpression_ComplexBinding_WrongValueSet_ThrowsExeption()
        {
            var target = new GenericBindingTarget<int>();
            var source1 = new GenericNotifyPropertyChanged<int>();
            var source2 = new GenericNotifyPropertyChanged<GenericNotifyPropertyChanged<int>>();
            source2.Value = new GenericNotifyPropertyChanged<int>();

            BindingEx.BindFromExpression(() => target.Value == source1.Value + source2.Value.Value);

            Assert.AreEqual(0, target.Value);
            source1.Value = 1;
            Assert.AreEqual(1, target.Value);
            source2.Value.Value = 1;
            Assert.AreEqual(2, target.Value);
            source2.Value = null;
        }
    }
}

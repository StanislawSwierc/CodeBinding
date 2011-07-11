CodeBinding
===========


Change log
----------
1.1.1.0

* Fixed bug related to implicit conversion when binding to enum types

1.1.0.0

* Supported binding to any properties (not DependencyProperties)
* New method 'IDisposable BindingEx.BindFromExpression<T>()' added
* Changed behavior of BindingEx.FromExpression<T>()
* Added test projects
* DelegateConverter and DelegateMultiValueConverter replaced with ExpressionValueConverter
* Improved binding creation engine
* Fixed bug related to not removing multi-bindings correctly

1.0.0.0

* Data Bindings creation from lambda expressions
* Supported binding to DependencyProperties


CodeBinding.Rx
==============

Change log
----------

1.1.1.0

* Updated dependency on CodeBinding

1.1.0.0

* Added test projects
* ObservableEx.FromExpression<T>() renamed to ObservableEx.Create<T>()
* Fixed problem with OnError method not being called when expression throws an exception

1.0.0.0

* IObservable<> object generation from lambda expressions

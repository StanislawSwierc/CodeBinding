CodeBinding
===========


Change log
----------

1.1.0.0 (work in progress)

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

1.1.0.0 (work in progress)

* Added test projects
* ObservableEx.FromExpression<T>() renamed to ObservableEx.Create<T>()

1.0.0.0

* IObservable<> object generation from lambda expressions

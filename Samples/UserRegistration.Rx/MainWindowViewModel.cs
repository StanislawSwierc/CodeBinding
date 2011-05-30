using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;
using System.Windows.Input;
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using System.Windows.Data;
using CodeBinding;
using CodeBinding.Rx;

namespace HelloReactiveUI
{
    class MainWindowViewModel : ReactiveObject
    {
        // This is ReactiveUI's version of implementing INotifyPropertyChanged
        string _Password;
        public string Password
        {
            get { return _Password; }
            set { this.RaiseAndSetIfChanged(x => x.Password, value); }
        }

        string _PasswordConfirmation;
        public string PasswordConfirmation
        {
            get { return _PasswordConfirmation; }
            set { this.RaiseAndSetIfChanged(x => x.PasswordConfirmation, value); }
        }
        

        public ICommand OkCommand { get; protected set; }
 
  
        public MainWindowViewModel()
        {
            // Here's the interesting part - we'll combine the change notifications
            // for Password and PasswordConfirmation, and that will determine when
            // we can hit the Ok button
            //
            var canHitOk = ObservableEx.FromExpression(() =>
                !string.IsNullOrEmpty(Password) &&
                Password == PasswordConfirmation &&
                Password.Length > 3);

            // Feed this to the canExecute of the OkCommand - now the button is
            // bound to the two properties and will disable itself until the
            // Func above is true.
            OkCommand = new ReactiveCommand(canHitOk);
        }
    }
}

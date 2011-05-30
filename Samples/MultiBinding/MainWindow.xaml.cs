using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeBinding;

namespace MultiBinding
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BindingEx.FromExpression(() => result.Text == (1000 * int.Parse(arg1.Text) + 100 * int.Parse(arg2.Text) + 10 * int.Parse(arg3.Text) + int.Parse(arg4.Text)).ToString());
        }
    }
}

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
            BindingEx.BindFromExpression(() => result.Text == (1000 * TryParse(arg1.Text, 0) + 100 * TryParse(arg2.Text, 0) + 10 * TryParse(arg3.Text, 0) + TryParse(arg4.Text, 0)).ToString());
        }

        public static int TryParse(string s, int fallback)
        {
            int result;
            if(int.TryParse(s, out result))
            {
                return result;
            }else
            {
                return fallback;
            }
        }
    }
}

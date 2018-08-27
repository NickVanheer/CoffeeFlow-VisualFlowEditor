using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CoffeeFlow.Annotations;

namespace CoffeeFlow.Views
{
    /// <summary>
    /// Interaction logic for CodeResultWindow.xaml
    /// </summary>
    public partial class CodeResultWindow : Window
    {
        public CodeResultWindow()
        {
            InitializeComponent();
            //DataContext = this;
        }
    }
}

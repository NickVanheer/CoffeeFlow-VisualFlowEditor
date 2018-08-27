using System.IO;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using CoffeeFlow.Base;
using CoffeeFlow.Nodes;
using CoffeeFlow.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoffeeFlow.Views;

namespace CoffeeFlow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RelayCommand _toggleSidebar;
        public RelayCommand ToggleSidebarCommand
        {
            get { return _toggleSidebar ?? (_toggleSidebar = new RelayCommand(ToggleSidebar)); }
        }

        private RelayCommand _hideAvailableNodesCommand;

        public RelayCommand HideNodeListCommand
        {
            get { return _hideAvailableNodesCommand ?? (_hideAvailableNodesCommand = new RelayCommand(HideNodeList)); }
        }

        private RelayCommand _CloseAppCommand;

        public RelayCommand CloseAppCommand
        {
            get { return _CloseAppCommand ?? (_CloseAppCommand = new RelayCommand(Close)); }
        }

        private RelayCommand _showAvailableNodesCommand;

        public RelayCommand ShowNodeListCommand
        {
            get { return _showAvailableNodesCommand ?? (_showAvailableNodesCommand = new RelayCommand(ShowNodeList)); }
        }

        public bool IsNodePopupVisible = false;

        public void ToggleSidebar()
        {
            if (GridColumn1.Visibility == System.Windows.Visibility.Collapsed)
            {
                GridColumn1.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                GridColumn1.Visibility = System.Windows.Visibility.Collapsed;
            }

            HideNodeList();
        }

        public Point GetMouseLocation()
        {
            return Mouse.GetPosition(Application.Current.MainWindow);
        }

        public void ShowAtMousePosition(FrameworkElement UI)
        {
            Point p = GetMouseLocation();
            UI.Visibility = Visibility.Visible;

            UI.Margin = new Thickness(p.X, p.Y, 0, 0);

            IsNodePopupVisible = true;
        }

        public void ShowNodeList()
        {
            ShowAtMousePosition(lstAvailableNodes);
        }

        public void HideNodeList()
        {
            lstAvailableNodes.Visibility = Visibility.Collapsed;
            IsNodePopupVisible = false;
        }


        public MainWindow()
        {
            InitializeComponent();

            NetworkViewModel v = SimpleIoc.Default.GetInstance<NetworkViewModel>();
            v.MainWindow = this;

            ToggleSidebar(); //turn sidebar off
            HideNodeList();
         
            /*
            InfoWindow info = new InfoWindow();
            info.ShowDialog();

            */
        }

      
    }
}

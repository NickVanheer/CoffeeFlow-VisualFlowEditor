using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using CoffeeFlow.Annotations;
using CoffeeFlow.Base;
using CoffeeFlow.Nodes;
using CoffeeFlow.ViewModel;
using UnityFlow;

namespace CoffeeFlow.Views
{
    /// <summary>
    /// Interaction logic for NodeListWindow.xaml
    /// </summary>
    public partial class NodeListWindow : UserControl, INotifyPropertyChanged
    {
        public bool IsSetup = false;

        Style bluestyle = Application.Current.FindResource("BlueButton") as Style;
        Style darkstyle = Application.Current.FindResource("DarkButton") as Style;

        public NodeListWindow()
        {
            InitializeComponent();

        }

        private RelayCommand _showTriggers;
        public RelayCommand ShowTriggersCommand
        {
            get { return _showTriggers ?? (_showTriggers = new RelayCommand(ShowTriggers)); }
        }

        private RelayCommand _showMethods;

        public RelayCommand ShowMethodsCommand
        {
            get { return _showMethods ?? (_showMethods = new RelayCommand(ShowMethods)); }
        }

        private RelayCommand _showVariables;

        public RelayCommand ShowVariablesCommand
        {
            get { return _showVariables ?? (_showVariables = new RelayCommand(ShowVariables)); }
        }

        public ObservableCollection<NodeWrapper> SearchCollection;

        public void ShowTriggers()
        {
            btnTriggers.Style = bluestyle;
            btnEvents.Style = darkstyle;
            btnInsight.Style = darkstyle;
            btnVariables.Style = darkstyle;

            MainViewModel m = SimpleIoc.Default.GetInstance<MainViewModel>();
            lstAvailableNodes.ItemsSource = m.Triggers;

            DisableSearch();
        }

        public void ShowMethods()
        {
            btnTriggers.Style = darkstyle;
            btnEvents.Style = darkstyle;
            btnInsight.Style = bluestyle;
            btnVariables.Style = darkstyle;

            MainViewModel m = SimpleIoc.Default.GetInstance<MainViewModel>();
            lstAvailableNodes.ItemsSource = m.Methods;

            DisableSearch();
        }

        public void ShowVariables()
        {
            btnTriggers.Style = darkstyle;
            btnEvents.Style = darkstyle;
            btnInsight.Style = darkstyle;
            btnVariables.Style = bluestyle;

            MainViewModel m = SimpleIoc.Default.GetInstance<MainViewModel>();
            lstAvailableNodes.ItemsSource = m.Variables;

            DisableSearch();
        }

        public void AddRootNode(string name)
        {
            RootNode root = new RootNode();
            root.Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        ObservableCollection<NodeWrapper> searchCopy = new ObservableCollection<NodeWrapper>();
        bool isSearching = false;
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isSearching == false)
            {
                //initiate a search
                searchCopy = lstAvailableNodes.ItemsSource as ObservableCollection<NodeWrapper>;
                isSearching = true;
            }
            
            if(isSearching)
            {
                string searchText = searchBox.Text;
                var result = from node in searchCopy
                             where node.NodeName.ToLower().Contains(searchText.ToLower())
                             select node;

                lstAvailableNodes.ItemsSource = result;
            }
        }

        public void DisableSearch()
        {
            isSearching = false;
            searchBox.Text = "";
        }

        private void nodeList_Loaded(object sender, RoutedEventArgs e)
        {
            if(!IsSetup)
            {
                SearchCollection = new ObservableCollection<NodeWrapper>();
                ShowTriggers();
            }
        }

        public static readonly DependencyProperty IsCenterProperty = DependencyProperty.Register(
    "IsCenter", typeof(bool), typeof(NodeListWindow));

        public bool IsCenter
        {
            get { return (bool)GetValue(IsCenterProperty); }
            set
            {
                SetValue(IsCenterProperty, value);

            }
        }
    }
}

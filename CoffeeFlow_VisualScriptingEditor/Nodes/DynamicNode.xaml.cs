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
using CoffeeFlow.Base;
using GalaSoft.MvvmLight.CommandWpf;
using System.Xml.Serialization;
using UnityFlow;

namespace CoffeeFlow.Nodes
{
    /// <summary>
    /// Interaction logic for DynamicNode.xaml
    /// </summary>
    /// 
    public partial class DynamicNode : NodeViewModel
    {
        public string Command;
        public Type ReturnType;

        public Connector InConnector;
        public Connector OutConnector;

        public string FullString;

        public int LabelWidth = 120;
        public int TextBoxWidth = 50;

        public Thickness BottomMargin = new Thickness(7, 0, 7, 5);

        public int NodeHeight = 80;
        public int TextBoxHeight = 100;
        public int NumericBoxHeight = 25;

        public int SmallNodeWidth = 210;
        public int LargeNodeWidth = 300;

        public int NodePanelHeight { get { return (int)this.Height; } set { this.Height = value; } }

        public List<Control> UIInputControls;
        public ObservableCollection<Argument> ArgumentCache;
        private List<TextBox> stringTextBoxes;
        private List<Connector> argConnectors;

        private RelayCommand _changeSize;
        public RelayCommand ChangeSizeCommand
        {
            get { return _changeSize ?? (_changeSize = new RelayCommand(ChangeSize)); }
        }

        public DynamicNode()
        {
            InitializeComponent();

            this.NodeType = NodeType.MethodNode;

            InExecutionConnector.ParentNode = (NodeViewModel)this;
            InExecutionConnector.TypeOfInputOutput = InputOutputType.Input;

            OutExecutionConnector.ParentNode = (NodeViewModel)this;
            OutExecutionConnector.TypeOfInputOutput = InputOutputType.Output;

            InConnector = InExecutionConnector;
            OutConnector = OutExecutionConnector;

            ArgumentCache = new ObservableCollection<Argument>();
            argConnectors = new List<Connector>();
            UIInputControls = new List<Control>();
            stringTextBoxes = new List<TextBox>();

            DataContext = this;

            this.Width = SmallNodeWidth;
        }

        public void ChangeSize()
        {
            if (this.Width == SmallNodeWidth)
            {
                this.Width = LargeNodeWidth;

                foreach (var item in stringTextBoxes)
                {
                    item.Height = TextBoxHeight;
                    this.Height += 80;
                }
            }
            else if (this.Width == LargeNodeWidth)
            {
                this.Width = SmallNodeWidth;

                foreach (var item in stringTextBoxes)
                {
                    item.Height = NumericBoxHeight;
                    this.Height -= 80;
                }
            }

            this.UpdateLayout();
        }

        public void DisableInputOnParameter(int parameterIndex)
        {
            if(UIInputControls.Count() > 0 && UIInputControls.Count() >= parameterIndex) 
                UIInputControls[parameterIndex].IsEnabled = false;

        }


        public void EnableInputOnParameter(int parameterIndex)
        {
            if (UIInputControls.Count() > 0 && UIInputControls.Count() >= parameterIndex)
                UIInputControls[parameterIndex].IsEnabled = true;

        }

        public void AddArgument(string type, string argumentName, bool isExisting, int connectedToNodeID = 0, object argumentValue = null)
        {
            bool isUnknown = true;
            if(type == "string")
            {
                var stackPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = BottomMargin };
                var stackPanel2 = new StackPanel { Orientation = Orientation.Horizontal, Margin = BottomMargin };

                TextBox value = new TextBox { Text = "Some value here.", HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch, TextWrapping = TextWrapping.Wrap, Height = NumericBoxHeight };
                Connector con = new Connector() { Name = "TextNodeConnector", TypeOfConnector = Base.ConnectorType.NodeParameter, ArgumentType = "string", Width = 15, Height = 15, Margin = new Thickness(0, 0, 5, 0), ParentNode = this, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, TypeOfInputOutput = InputOutputType.Input, OrderOfArgumentID = ArgumentCache.Count() };
                argConnectors.Add(con);

                stackPanel2.Margin = new Thickness(0, 0, 7, 5);
                stackPanel2.Children.Add(con);
                stackPanel2.Children.Add(new TextBlock { Text = argumentName + ":", Width = LabelWidth, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Foreground = new SolidColorBrush(Colors.White) });

                stackPanel.Children.Add(stackPanel2);
                stackPanel.Children.Add(value);
         
                ArgumentList.Children.Add(stackPanel);
                stringTextBoxes.Add(value);

                UIInputControls.Add(value);

                //
                Argument behind = new Argument(argumentName, type);
                if(isExisting)
                {
                    behind.ArgIsExistingVariable = true;
                    behind.ArgumentConnectedToNodeID = connectedToNodeID;
                }

                behind.ArgValue = "Some value here.";

                if (argumentValue != null)
                    behind.ArgValue = argumentValue;

                ArgumentCache.Add(behind);

                //binding 
                Binding myBinding = new Binding();
                myBinding.Source = behind;
                myBinding.Path = new PropertyPath("ArgValue");
                myBinding.Mode = BindingMode.TwoWay;
                myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(value, TextBox.TextProperty, myBinding);

                NodeHeight += NumericBoxHeight + 10;
                isUnknown = false;
            }

            if (type == "float")
            {
                //UI
                var dockPanel = new DockPanel { Margin = BottomMargin, Height = NumericBoxHeight, HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch };
                Connector con = new Connector() { Width = 15, TypeOfConnector = Base.ConnectorType.NodeParameter, ArgumentType = "float", Height = 15, Margin = new Thickness(0, 0, 5, 0), ParentNode = this, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, TypeOfInputOutput = InputOutputType.Input, OrderOfArgumentID = ArgumentCache.Count() };
                argConnectors.Add(con);

                dockPanel.Children.Add(con);
                dockPanel.Children.Add(new TextBlock { Text = argumentName + ":", Width = LabelWidth, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Foreground = new SolidColorBrush(Colors.White) });
                TextBox value = new TextBox { Text = "", HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch };
                dockPanel.Children.Add(value);
                ArgumentList.Children.Add(dockPanel);

                UIInputControls.Add(value);

                //Code-behind            
                Argument behind = new Argument(argumentName, type);
                if (isExisting)
                {
                    behind.ArgIsExistingVariable = true;
                    behind.ArgumentConnectedToNodeID = connectedToNodeID;
                }
                behind.ArgValue = 2.5f;

                if (argumentValue != null)
                    behind.ArgValue = argumentValue;

                ArgumentCache.Add(behind);

                //binding 
                Binding myBinding = new Binding();
                myBinding.Source = behind;
                myBinding.Path = new PropertyPath("ArgValue");
                myBinding.Mode = BindingMode.TwoWay;
                myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(value, TextBox.TextProperty, myBinding);

                NodeHeight += NumericBoxHeight + 10;
                isUnknown = false;
            }

            if (type == "int")
            {
                //UI
                var dockPanel = new DockPanel { Margin = BottomMargin, Height = NumericBoxHeight, HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch };
                Connector con = new Connector() { Width = 15, TypeOfConnector = Base.ConnectorType.NodeParameter, ArgumentType = "int", Height = 15, Margin = new Thickness(0, 0, 5, 0), ParentNode = this, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, TypeOfInputOutput = InputOutputType.Input, OrderOfArgumentID = ArgumentCache.Count() };
                argConnectors.Add(con);

                dockPanel.Children.Add(con);
                dockPanel.Children.Add(new TextBlock { Text = argumentName + ":", Width = LabelWidth, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Foreground = new SolidColorBrush(Colors.White) });
                TextBox value = new TextBox { Text = "", HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch };
                dockPanel.Children.Add(value);
                ArgumentList.Children.Add(dockPanel);


                UIInputControls.Add(value);

                //Code-behind            
                Argument behind = new Argument(argumentName, type);
                behind.ArgValue = 1;
                if (isExisting)
                {
                    behind.ArgIsExistingVariable = true;
                    behind.ArgumentConnectedToNodeID = connectedToNodeID;
                }


                if (argumentValue != null)
                    behind.ArgValue = argumentValue;

                ArgumentCache.Add(behind);

                //binding 
                Binding myBinding = new Binding();
                myBinding.Source = behind;
                myBinding.Path = new PropertyPath("ArgValue");
                myBinding.Mode = BindingMode.TwoWay;
                myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(value, TextBox.TextProperty, myBinding);

                NodeHeight += NumericBoxHeight + 10;
                isUnknown = false;
            }

            if (type == "bool")
            {
                //UI
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = BottomMargin, Height = NumericBoxHeight };
                Connector con = new Connector() { Width = 15, TypeOfConnector = Base.ConnectorType.NodeParameter, ArgumentType = "bool", Height = 15, Margin = new Thickness(0, 0, 5, 0), ParentNode = this, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, TypeOfInputOutput = InputOutputType.Input, OrderOfArgumentID = ArgumentCache.Count() };
                argConnectors.Add(con);

                stackPanel.Children.Add(con);
                
                stackPanel.Children.Add(new TextBlock { Text = argumentName + ":", Width = LabelWidth, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Foreground = new SolidColorBrush(Colors.White) });
                CheckBox value = new CheckBox();
                stackPanel.Children.Add(value);
                ArgumentList.Children.Add(stackPanel);

                UIInputControls.Add(value);

                //Code-behind            
                Argument behind = new Argument(argumentName, type);
                behind.ArgValue = true;
                ArgumentCache.Add(behind);

                //binding 
                Binding myBinding = new Binding();
                myBinding.Source = behind;
                myBinding.Path = new PropertyPath("ArgValue");
                myBinding.Mode = BindingMode.TwoWay;
                myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(value, CheckBox.IsCheckedProperty, myBinding);

                NodeHeight += NumericBoxHeight + 10;
                isUnknown = false;
            }

            if(isUnknown)
            {
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = BottomMargin, Height = NumericBoxHeight };
                Connector con = new Connector() { Width = 15, TypeOfConnector = Base.ConnectorType.NodeParameter, ArgumentType = type, Height = 15, Margin = new Thickness(0, 0, 5, 0), ParentNode = this, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, TypeOfInputOutput = InputOutputType.Input, OrderOfArgumentID = ArgumentCache.Count(), IsNoLinkedInputField = true };
                argConnectors.Add(con);

                stackPanel.Children.Add(con);
                stackPanel.Children.Add(new TextBlock { Text = argumentName + " (" + type + ")", Width = LabelWidth, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Foreground = new SolidColorBrush(Colors.White) });
                ArgumentList.Children.Add(stackPanel);
                
                //Code-behind            
                Argument behind = new Argument(argumentName, type);
                behind.ArgValue = null;
                if (isExisting)
                {
                    behind.ArgIsExistingVariable = true;
                    behind.ArgumentConnectedToNodeID = connectedToNodeID;
                }

                ArgumentCache.Add(behind);

                NodeHeight += NumericBoxHeight + 10;
            }

            //TODO: Enum
            this.Height = NodeHeight;
            ArgumentList.UpdateLayout();
        }

        public override void Populate(SerializeableNodeViewModel node)
        {
            base.Populate(node);

            //connectors
            this.InExecutionConnector.ConnectionNodeID = (node as SerializeableDynamicNode).InputNodeID;
            this.OutExecutionConnector.ConnectionNodeID = (node as SerializeableDynamicNode).OutputNodeID;

            var arguments = (node as SerializeableDynamicNode).Arguments;

            foreach (var arg in arguments)
            {
                AddArgument(arg.ArgTypeString, arg.Name, arg.ArgIsExistingVariable, arg.ArgumentConnectedToNodeID, arg.ArgValue);
            }

            this.CallingClass = node.CallingClass;
        }

        public override string ToString()
        {
            return NodeName;
        }

        public Connector GetConnectorAtIndex(int index)
        {
            return argConnectors.ElementAt(index);
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}

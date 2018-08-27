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
    public partial class ConditionNode : NodeViewModel
    {
        public string FullString;
        public string ConnectedToVariableName { get; set; }
        public string ConnectedToVariableCallerClassName { get; set; }

        public ConditionNode()
        {
            InitializeComponent();

            this.NodeType = NodeType.ConditionNode;

            InExecutionConnector.ParentNode = (NodeViewModel)this;
            InExecutionConnector.TypeOfInputOutput = InputOutputType.Input;

            OutExecutionConnectorTrue.ParentNode = (NodeViewModel)this;
            OutExecutionConnectorTrue.TypeOfInputOutput = InputOutputType.Output;

            OutExecutionConnectorFalse.ParentNode = (NodeViewModel)this;
            OutExecutionConnectorFalse.TypeOfInputOutput = InputOutputType.Output;

            boolInput.ParentNode = (NodeViewModel)this;
            boolInput.TypeOfInputOutput = InputOutputType.Input;

            DataContext = this;
        }

        public override void Populate(SerializeableNodeViewModel node)
        {
            base.Populate(node);

            SerializeableConditionNode ser = (node as SerializeableConditionNode);
            this.InExecutionConnector.ConnectionNodeID = ser.InputNodeID;
            this.OutExecutionConnectorFalse.ConnectionNodeID = ser.OutputFalseNodeID;
            this.OutExecutionConnectorTrue.ConnectionNodeID = ser.OutputTrueNodeID;
            this.boolInput.ConnectionNodeID = ser.BoolVariableID;
            this.ConnectedToVariableCallerClassName = ser.BoolCallingClass;   
         
            this.CallingClass = node.CallingClass;
        }

        public override string ToString()
        {
            return NodeName.ToString();
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

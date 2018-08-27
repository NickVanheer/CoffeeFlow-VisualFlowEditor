using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CoffeeFlow.Base;
using UnityFlow;

namespace CoffeeFlow.Nodes
{
    /// <summary>
    /// Interaction logic for VariableNode.xaml
    /// </summary>
    /// 

    public partial class VariableNode : NodeViewModel
    {
        private string _type;

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value; 
                OnPropertyChanged("Type");
            }
        }

        public VariableKind KindOfVariable;

        public object TypeValue { get; set; }
        public bool IsInitialised
        {
            get { return TypeValue != null; }
        }

        public VariableNode()
        {
            InitializeComponent();

            this.Type = "string";
            this.NodeType = NodeType.VariableNode;

            this.NodeParameterOut.ParentNode = (NodeViewModel) this;
            this.NodeParameterOut.TypeOfInputOutput = InputOutputType.Output;
            this.NodeParameterOut.TypeOfConnector = ConnectorType.VariableConnector;
            
            KindOfVariable = VariableKind.Field;
            DataContext = this;
        }

        public override void Populate(SerializeableNodeViewModel node)
        {
            base.Populate(node);
            SerializeableVariableNode v = node as SerializeableVariableNode;

            this.Type = v.TypeString;
            this.CallingClass = node.CallingClass;
        }

        public override string ToString()
        {
            return this.NodeName;
        }
    }
}

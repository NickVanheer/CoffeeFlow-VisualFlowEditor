using System;
using System.Collections.Generic;
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
using UnityFlow;

namespace CoffeeFlow.Nodes
{
    /// <summary>
    /// Interaction logic for OperatorNode.xaml
    /// </summary>
    public partial class SetNode : NodeViewModel
    {
        public string Operator { get; set; }

        public SetNode()
        {
            InitializeComponent();

            InExecutionConnector.ParentNode = (NodeViewModel)this;
            OutExecutionConnector.ParentNode = (NodeViewModel)this;

            this.InExecutionConnector.TypeOfInputOutput = InputOutputType.Input;
            this.OutExecutionConnector.TypeOfInputOutput = InputOutputType.Output;

            this.NodeName = "Fire Trigger";
            this.NodeDescription = "Fires the specified trigger, which the user can intercept in code";
        }

        public override void Populate(SerializeableNodeViewModel node)
        {
            throw new NotImplementedException();
        }
    }
}

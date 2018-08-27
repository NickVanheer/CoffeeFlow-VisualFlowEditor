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
    /// Interaction logic for RootNode.xaml
    /// </summary>
    /// 


    public partial class RootNode : NodeViewModel
    {
        public Connector OutputConnector;
        public string EventSourceNodeName;

        public RootNode()
        {
            InitializeComponent();

            this.NodeName = "ROOTNODE";
            this.MainConnector.ParentNode = (NodeViewModel)this;
            this.MainConnector.TypeOfInputOutput = InputOutputType.Output;

            OutputConnector = this.MainConnector;

            DataContext = this;
        }

        public RootNode GetCopy()
        {
            RootNode newNode = new RootNode();
            newNode.NodeName = this.NodeName;

            return newNode;
        }

        public override void Populate(SerializeableNodeViewModel node)
        {
            base.Populate(node);
            this.MainConnector.ConnectionNodeID = (node as SerializeableRootNode).OutputNodeID;
        }
    }
}

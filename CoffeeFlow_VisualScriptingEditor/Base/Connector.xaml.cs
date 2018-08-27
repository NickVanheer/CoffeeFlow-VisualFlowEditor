using CoffeeFlow.Nodes;
using CoffeeFlow.ViewModel;
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
using UnityFlow;

namespace CoffeeFlow.Base
{
    /// <summary>
    /// Interaction logic for Connector.xaml
    /// </summary>
    /// 

    public enum InputOutputType
    {
        Input, Output,
    };

    public enum ConnectorType
    {
        ExecutionFlow, NodeParameter, VariableConnector
    };

    [Serializable]
    public class ConnectorSerializeable
    {
        public bool IsConnected;

        /// <summary>
        /// This connector's Nod
        /// </summary>
        public int ParentNodeID;

        /// <summary>
        /// This connector's ID
        /// </summary>
        public int ID;

        /// <summary>
        /// connector ID that this connector is connected to
        /// </summary>
        public int ConnectedToID;
    }

    public partial class Connector : UserControl, INotifyPropertyChanged
    {
        public NodeViewModel ParentNode;
        public bool IsConnected = false;
        public Connector Connection;

        public static List<Connector> Connectors = new List<Connector>(); 

        public int ParentNodeID
        {
            get { return ParentNode.ID; }
        }
        public int ID;
        public int ConnectedToConnectorID;
        public static int MaxID = 0;

        public int ConnectionNodeID = 0;

        public int OrderOfArgumentID;


        public NodeViewModel ConnectionNode
        {
            get
            {
                if (Connection == null || !IsConnected)
                    return null;

                if (Connection.ParentNode != null)
                    return Connection.ParentNode;
                else
                    return null;
            }
        }

        public InputOutputType TypeOfInputOutput;
        public ConnectorType TypeOfConnector;

        //public static bool IsConnecting = false;
        public static Connector CurrentConnection;

        private SolidColorBrush hoverColor;
        public SolidColorBrush HoverColor
        {
            get { return hoverColor; }
            set
            {
                hoverColor = value;
                NotifyPropertyChanged("HoverColor");
            }
        }

        private SolidColorBrush hoverDefaultColor;
        private SolidColorBrush hoverAcceptColor;
        private SolidColorBrush hoverDenyColor;

        public Brush baseColor = Brushes.Black;

       

        public static readonly DependencyProperty ConnectorTypeProperty = DependencyProperty.Register(
            "ConnectorType", typeof(ConnectorType), typeof(Connector), new PropertyMetadata(default(ConnectorType)));

        public ConnectorType ConnectorType
        {
            get { return (ConnectorType)GetValue(ConnectorTypeProperty); }
            set
            {
                SetValue(ConnectorTypeProperty, value);

            }
        }

        public static readonly DependencyProperty IsNoLinkedInputFieldProperty = DependencyProperty.Register(
      "IsNoLinkedInputField", typeof(bool), typeof(Connector), new PropertyMetadata(false));

        public bool IsNoLinkedInputField
        {
            get { return (bool)GetValue(IsNoLinkedInputFieldProperty); }
            set
            {
                SetValue(IsNoLinkedInputFieldProperty, value);

            }
        }

        public static readonly DependencyProperty ArgumentTypeProperty = DependencyProperty.Register(
"ArgumentType", typeof(string), typeof(Connector), new PropertyMetadata(""));

        public string ArgumentType
        {
            get { return (string)GetValue(ArgumentTypeProperty); }
            set
            {
                SetValue(ArgumentTypeProperty, value);

            }
        }

        //public string ArgumentType;

        public Connector()
        {
            InitializeComponent();
            HandleEvents();

            baseColor = Brushes.Black;
            //BackgroundGrid.Background = baseColor;

            Connection = null;
            TypeOfConnector = Base.ConnectorType.ExecutionFlow;

            MaxID++;
            ID = MaxID;

            Connectors.Add(this);

            
            hoverDefaultColor = new SolidColorBrush(Colors.Yellow);
            hoverAcceptColor = new SolidColorBrush(Colors.Green);
            hoverDenyColor = new SolidColorBrush(Colors.Red);

            HoverColor = hoverDefaultColor;
        }


        public void SetID(int newID)
        {
            this.ID = newID;

            if (newID > MaxID)
                Connector.MaxID = newID;
        }


        public bool CheckConnection(Connector first, Connector second)
        {
            //TODO: improve when connecting the other way around

            // Check for connection type and attachments
            if (first == second)
                return false;

            if (first.TypeOfInputOutput == InputOutputType.Output && second.TypeOfInputOutput == InputOutputType.Output)
                return false;

            if (first.TypeOfInputOutput == InputOutputType.Input && second.TypeOfInputOutput == InputOutputType.Input)
                return false;
    
            //make sure we don't connect 2 methods
            NodeViewModel n1 = first.ParentNode;
            NodeViewModel n2 = second.ParentNode;

            //trying to connect a parameter connector to a execution flow, not allowed.
            if (first.TypeOfConnector == Base.ConnectorType.NodeParameter && second.TypeOfConnector == Base.ConnectorType.ExecutionFlow)
                return false;

            //trying to connect a parameter connector to a execution flow, not allowed.
            if (second.TypeOfConnector == Base.ConnectorType.NodeParameter && first.TypeOfConnector == Base.ConnectorType.ExecutionFlow)
                return false;

            //In case of variables
            if(n1.NodeType == NodeType.VariableNode)
            {
                VariableNode v = n1 as VariableNode;
                if (second.ArgumentType != v.Type)
                    return false;
            }
            if (n2.NodeType == NodeType.VariableNode)
            {
                VariableNode v = n2 as VariableNode;
                if (first.ArgumentType != v.Type)
                    return false;
            }

         //TODO additional checks
        /*
        if (first.TypeOfInputOutput == second.TypeOfInputOutput)
            return false;
        */


            return true;
        }

        public static void StopConnecting()
        {
            if (CurrentConnection == null)
                return;

            CurrentConnection.baseColor = Brushes.Black;
            CurrentConnection.ParentNode.IsDraggable = true;
            CurrentConnection = null;
        }

        public void ClearConnection()
        {
            if (Connection == null)
                return;

            Connection.IsConnected = false;
            Connection.Connection = null; //huh?
            IsConnected = false;
            Connection = null;
        }

        

        public void ConnectMethodToNamedVariable(Connector paramOnMethodConnector, Connector variableConnector)
        {
            int paramIndex = paramOnMethodConnector.OrderOfArgumentID;

            DynamicNode n = paramOnMethodConnector.ParentNode as DynamicNode;
            VariableNode v = variableConnector.ParentNode as VariableNode;

            n.ArgumentCache[paramIndex].ArgIsExistingVariable = true;
            n.ArgumentCache[paramIndex].ArgValue = "";
            n.ArgumentCache[paramIndex].ArgExistingVariableName = v.NodeName;
            n.ArgumentCache[paramIndex].ArgumentConnectedToNodeID = v.ID;

            if (!paramOnMethodConnector.IsNoLinkedInputField)
            {
                n.DisableInputOnParameter(paramIndex);
            }

            MainViewModel.Instance.LogStatus("Adding linked parameter connection on " + n.NodeName + ", parameter " + paramIndex);

        }

        public void ConnectConditionToNamedVariable(Connector paramOnConditionConnector, Connector variableConnector)
        {
            ConditionNode n = paramOnConditionConnector.ParentNode as ConditionNode;
            VariableNode v = variableConnector.ParentNode as VariableNode;

            n.ConnectedToVariableName = v.NodeName;
            n.ConnectedToVariableCallerClassName = v.CallingClass;
            MainViewModel.Instance.LogStatus("Connected bool variable to Condition bool input");

        }

        public void RemoveLinkedParameterFromVariableNode()
        {
            int paramIndex = this.Connection.OrderOfArgumentID;

            if(this.Connection.ParentNode.NodeType == NodeType.ConditionNode)
            {
                ConditionNode n = this.Connection.ParentNode as ConditionNode;
                n.ConnectedToVariableName = "";
                n.ConnectedToVariableCallerClassName = "";
            }

            if (this.Connection.ParentNode.NodeType == NodeType.MethodNode)
            {
                DynamicNode n = this.Connection.ParentNode as DynamicNode;
                n.ArgumentCache[paramIndex].ArgIsExistingVariable = false;
                n.ArgumentCache[paramIndex].ArgExistingVariableName = "";

                if (!this.IsNoLinkedInputField)
                {
                    n.EnableInputOnParameter(paramIndex);
                }
            }

            //MainViewModel.Instance.LogStatus("Removing linked parameter connection on " + n.NodeName + ", parameter " + paramIndex);
        }

        public void UnConnect()
        {
            this.IsConnected = false;
            this.Connection = null;
        }

        public static void ConnectPins(Connector source, Connector target)
        {
            //
            if (source.IsConnected && source.ParentNode.NodeType == NodeType.VariableNode)
                source.RemoveLinkedParameterFromVariableNode();

            if (target.IsConnected && target.ParentNode.NodeType == NodeType.VariableNode)
                target.RemoveLinkedParameterFromVariableNode();

            if (source.IsConnected && !target.IsConnected)
                source.UnConnect();

            if (target.IsConnected && !source.IsConnected)
                target.UnConnect();

            int a = target.ParentNode.ID; //1
            int b = source.ParentNode.ID; //2

            source.ConnectionNodeID = a; //2 links to 1
            target.ConnectionNodeID = b; //1 links to 2

            source.ParentNode.IsDraggable = true;
            target.ParentNode.IsDraggable = true;

            target.ConnectedToConnectorID = source.ID;
            source.ConnectedToConnectorID = target.ID;

            //connects the two nodes
            source.Connection = target;
            target.Connection = source;
            target.IsConnected = true;
            source.IsConnected = true;

            MainViewModel.Instance.LogStatus("Connected node " + source.ParentNode + " (" + source.ParentNode.ID + ") to " + target.ParentNode + "(" + target.ParentNode.ID + ")");

            //variable as a method parameter
            if (source.ParentNode.NodeType == NodeType.VariableNode && target.ParentNode.NodeType == NodeType.MethodNode)
            {
                source.ConnectMethodToNamedVariable(target, source);
                MainViewModel.Instance.LogStatus("Connected variable to named parameter");
            }
            if (source.ParentNode.NodeType == NodeType.MethodNode && target.ParentNode.NodeType == NodeType.VariableNode)
            {
                source.ConnectMethodToNamedVariable(source, target);
                MainViewModel.Instance.LogStatus("Connected variable to named parameter");
            }

            //

            //variable as a condition input
            if (source.ParentNode.NodeType == NodeType.VariableNode && target.ParentNode.NodeType == NodeType.ConditionNode)
            {
                source.ConnectConditionToNamedVariable(target, source);
            }
            if (source.ParentNode.NodeType == NodeType.ConditionNode && target.ParentNode.NodeType == NodeType.VariableNode)
            {
                source.ConnectConditionToNamedVariable(source, target);
            }
        }
     
        public void HandleEvents()
        {
            
            //MOUSE ENTER
            this.MouseEnter += (a, b) =>
            {
                if (CurrentConnection != null)
                {
                    if(CheckConnection(this, CurrentConnection))
                    {
                        HoverColor = hoverAcceptColor;
                    }
                    else
                    {
                        HoverColor = hoverDenyColor;
                    }
                }

            };

            //MOUSE LEAVE
            this.MouseLeave += (a, b) =>
            {
                HoverColor = hoverDefaultColor;
            };

            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentConnection != null)
            {
                //There's a connection, connect this with current node
                    //ConnectToPin(CurrentConnection);

                    // Someone wants to connect with this pin
                    if (CheckConnection(this, CurrentConnection))
                    {
                        /* if (Connection != null)
                            Connection.ClearConnection();
                        */

                        //ConnectToPin(CurrentConnection);
                        ConnectPins(this, CurrentConnection);
                    }

                    CurrentConnection = null;
                    //ParentNode.IsDraggable = true;
               

            }
            else
            {
                //No connection, start one
                //source = (UIElement)sender;
                //Mouse.Capture(source);
                CurrentConnection = this;

                
                //ParentNode.IsDraggable = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}

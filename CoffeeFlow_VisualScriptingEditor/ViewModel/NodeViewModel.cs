using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using CoffeeFlow.Annotations;
using CoffeeFlow.Nodes;
using UnityFlow;

namespace CoffeeFlow.Base
{
    /**********************************************************************************************************
   *             Logic related to a single node on grid, handles core data and dragging, used as base class in other nodes
   * 
   *                                                      * Nick @ http://immersivenick.wordpress.com 
   *                                                      * Free for non-commercial use
   * *********************************************************************************************************/
    public abstract partial class NodeViewModel : UserControl, INotifyPropertyChanged
    {
        private double _scale;

        public double Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public NodeType NodeType
        {
            get { return _nodeType; }
            set
            {
                _nodeType = value;
                OnPropertyChanged("NodeType");
    
            }
        }

        private NodeType _nodeType;

        public virtual string GetSerializationString()
        {
            return "";
        }

        public string NodeDataString { get; set; }
        public string NodeDescription { get; set; }
        public bool CanDrag = true;

        private string callingClass;
        public string CallingClass
        {
            get { return callingClass; }
            set
            {
                callingClass = value;
                OnPropertyChanged("CallingClass");

            }
        }

        private int id;
        public int ID
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("ID");

            }
        }
        private static int TotalIDCount = 0;

        public string NodeName
        {
            get { return _nodeName; }
            set
            {

                _nodeName = value;
                OnPropertyChanged("NodeName");
                
            }
        }

        public string Debug { get; set; }

        public Double X { get; set; }
        public Double Y { get; set; }

        public bool IsDraggable = true;
        public TranslateTransform Transform { get; set; }
        public bool IsMouseDown;
        private string _nodeName;


        public static NodeViewModel Selected { get; set; }

        public static bool IsNodeDragging = false;
        public static double GlobalScaleDelta { get; set; }

        public ScaleTransform ScaleTransform { get; private set; }

        public virtual void Populate(SerializeableNodeViewModel node)
        {
            this.ID = node.ID;
            this.NodeName = node.NodeName;
            this.Margin = new Thickness(node.MarginX, node.MarginY, 0, 0);
        }

        public bool IsSelected {
            get
            {
                if(NodeViewModel.Selected != null)
                    return NodeViewModel.Selected == this;
                else
                {
                    return false;
                }
            }}

        public NodeViewModel()
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;      

            this.BorderBrush = new SolidColorBrush(Colors.Red);

            TotalIDCount++;
            ID = TotalIDCount;
            Scale = 1;
            MakeDraggable(this, this);

            DataContext = this;

            ScaleBy(GlobalScaleDelta);
        }

        public void ScaleBy(double increment)
        {
            Scale += increment;
            ScaleTransform.ScaleX = Scale;
            ScaleTransform.ScaleY = Scale;
        }


        bool captured = false;
        UIElement source = null;

        public void MakeDraggable(System.Windows.UIElement moveThisElement, System.Windows.UIElement movedByElement)
        {
            ScaleTransform scaleTransform = new ScaleTransform(Scale, Scale);
            TranslateTransform transform = new TranslateTransform(0, 0);

            ScaleTransform = scaleTransform;

            TransformGroup group = new TransformGroup();
            group.Children.Add(scaleTransform);
            group.Children.Add(transform); 

            moveThisElement.RenderTransform = group;
            
            this.Transform = transform;

            System.Windows.Point originalPoint = new System.Windows.Point(0, 0), currentPoint;
            
            //
            movedByElement.MouseLeftButtonDown += (sender, b) =>
            {
                source = (UIElement)sender;
                Mouse.Capture(source);
                captured = true;

                IsNodeDragging = true;
                originalPoint = ((System.Windows.Input.MouseEventArgs)b).GetPosition(moveThisElement);
            };

            movedByElement.MouseLeftButtonUp += (a, b) =>
                {
                    Mouse.Capture(null);
                    captured = false;

                    IsNodeDragging = false;
                };

            movedByElement.MouseMove += (a, b) =>
            {
                if (!IsDraggable) return;

                if (captured)
                {
                    currentPoint = ((System.Windows.Input.MouseEventArgs)b).GetPosition(moveThisElement);

                    transform.X += currentPoint.X - originalPoint.X;
                    transform.Y += currentPoint.Y - originalPoint.Y;
                }
            };
 
        }

        public void DisconnectAllConnectors()
        {
            var connectors = FindVisualChildren<Connector>(this);

            foreach (var connector in connectors)
            {
                if (connector.IsConnected)
                {
                    connector.Connection.IsConnected = false;
                    
                    //disconnecting variable
                    if(connector.ParentNode.NodeType == NodeType.VariableNode)
                        connector.RemoveLinkedParameterFromVariableNode();
                }
            }
        }

        //finds children
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

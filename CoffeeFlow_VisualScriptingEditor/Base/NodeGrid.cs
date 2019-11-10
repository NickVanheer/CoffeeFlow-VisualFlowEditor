using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Ioc;
using CoffeeFlow.Nodes;
using CoffeeFlow.ViewModel;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using System.Diagnostics;

namespace CoffeeFlow.Base
{
    /**********************************************************************************************************
     *             Logic related to the grid itself and its manipulation like moving, zooming and drawing of node connections
     * 
     *                                                      * Nick @ http://immersivenick.wordpress.com 
     *                                                      * Free for non-commercial use
     * *********************************************************************************************************/
    public class NodeGrid : ItemsControl
    {
        private DateTime previousDrawTime;

        //Grid Properties
        public int gridSize = 0;
        public int defaultGridSize = 40;
        public Point GridOffset = new Point(0, 0);
        public string DebugTest = "";

        //
        private Point lastGridOffset = new Point(0, 0);
        private Point defaultGridOffset = new Point(0, 0);
        private Point gridLastPos = new Point(0, 0);

        private bool DragMouseDown = false;
        private BezierCurve bezierCurve = new BezierCurve();

        private SolidColorBrush GridConnectionColor;

        private NetworkViewModel networkView;
        private double elapsedTime;
        private SolidColorBrush GridColor = new SolidColorBrush(Color.FromArgb(255, (byte)50, (byte)50, (byte)50));
        const int POINTS_ON_CURVE = 30;

        public Point GetAbsoluteCenter
        {
            get
            {
                return new Point(RenderSize.Width / 2, RenderSize.Height / 2);
            }
        }
 
        public NodeGrid()
        {
            //InitializeComponent();

            FrameworkElementFactory factoryPanel = new FrameworkElementFactory(typeof(Canvas));
            factoryPanel.SetValue(Canvas.IsItemsHostProperty, true);
            ItemsPanelTemplate template = new ItemsPanelTemplate();
            template.VisualTree = factoryPanel;

            this.ItemsPanel = template;

            GridConnectionColor = new SolidColorBrush(Color.FromRgb(150, 150, 150));

            previousDrawTime = DateTime.Now;

            gridSize = defaultGridSize;
            GridOffset = defaultGridOffset;

            var paintTimer = new Timer
            {
                AutoReset = true,
                Interval = 1000.0 / 60
            };

            paintTimer.Elapsed += (o, e) => Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(InvalidateVisual));
            paintTimer.Start();

            this.MouseMove += OnMouseMove;
            this.MouseDown += OnMouseDown;
            this.MouseUp += OnMouseUp;
            this.MouseWheel += OnMouseWheel;

            NodeViewModel.GlobalScaleDelta = 0;

            if (networkView == null)
                networkView = SimpleIoc.Default.GetInstance<NetworkViewModel>();

            elapsedTime = 0;

        }

       
        protected override void OnRender(DrawingContext drawingContext)
        {
            /*
            var now = DateTime.Now;
            var difference = now - previousDrawTime;
            previousDrawTime = now;

            var deltaTime = difference.TotalSeconds;
            elapsedTime += deltaTime;
            */
  
            Paint(drawingContext);
            base.OnRender(drawingContext);
        }

        private void Paint(DrawingContext drawingContext)
        {
            // Background
            drawingContext.DrawRectangle(GridColor, null, new Rect(0, 0, RenderSize.Width, RenderSize.Height));

            // Grid
            DrawGrid(drawingContext);

            // Connections
            DrawConnections(drawingContext);

            DrawCurrentConnection(drawingContext);
        }

        private void DrawGrid(DrawingContext drawingContext)
        {
            int lineWidth = (int)RenderSize.Width;
            int lineHeight = (int)RenderSize.Height;

            int lineAmountX = (int)lineWidth / gridSize + 1;
            int lineAmountY = (int)lineHeight / gridSize + 1;

            Brush color = new SolidColorBrush(Color.FromArgb(255, (byte)80, (byte)80, (byte)80));
            int thickness = 1;

            Point relativeOffset = new Point(GridOffset.X % gridSize, GridOffset.Y % gridSize);

            for (int x = 0; x < lineAmountX; x++)
            {
                drawingContext.DrawLine(new Pen(color, thickness), new Point(x * gridSize + relativeOffset.X, 0), new Point(x * gridSize + relativeOffset.X, lineHeight));
            }

            for (int y = 0; y < lineAmountY; y++)
            {
                drawingContext.DrawLine(new Pen(color, thickness), new Point(0, y * gridSize + relativeOffset.Y), new Point(lineWidth, y * gridSize + relativeOffset.Y));
            }

            if (ItemsSource != null)
            {
                foreach (var nodeObj in ItemsSource)
                {
                    var node = nodeObj as NodeViewModel;

                    var transform = node.Transform;

                    if (!node.IsMouseDown)
                    {
                        if (GridOffset != lastGridOffset)
                        {
                            transform.X += GridOffset.X - lastGridOffset.X;
                            transform.Y += GridOffset.Y - lastGridOffset.Y;
                        }
                    }
                }
            }

            lastGridOffset = GridOffset;
        }

        private void DrawCurrentConnection(DrawingContext drawingContext)
        {
            int thickness = 2;
            Brush color = System.Windows.Media.Brushes.DarkGray;

            // Draw the current connection, if any
            if (Connector.CurrentConnection != null)
            {
                var currentPin = Connector.CurrentConnection;
                var first = currentPin.TransformToAncestor(MainWindow.GetWindow(this)).Transform(new Point(0, 0));

                // Second is the cursor position
                var second = Mouse.GetPosition(this);

                first.X += currentPin.Width / 2;
                first.Y += currentPin.Height / 2;

                // Swap if we are connecting an input
                if (currentPin.TypeOfInputOutput == InputOutputType.Input)
                {
                    var temp = first;
                    first = second;
                    second = temp;
                }

                int connectionStrenght = networkView.BezierStrength;

                double[] ptind = new double[] {     first.X, first.Y,
                                                    first.X + connectionStrenght, first.Y,
                                                    second.X - connectionStrenght, second.Y,
                                                    second.X, second.Y};

                double[] p = new double[POINTS_ON_CURVE];

                bezierCurve.Bezier2D(ptind, (POINTS_ON_CURVE) / 2, p);

                // draw points
                for (int i = 1; i != POINTS_ON_CURVE - 3; i += 2)
                {
                    drawingContext.DrawLine(new Pen(color, thickness), new Point((int)p[i + 1], (int)p[i]), new Point((int)p[i + 3], (int)p[i + 2]));
                }
            }

        }

        float ADDX = -5;
        float ADDY = -15;
        //Input: left, top, output: right, top
        private void DrawConnections(DrawingContext drawingContext)
        {
            int thickness = 1;
            Brush color = System.Windows.Media.Brushes.DarkGray;

            //draw all connections
            // Draw all the connections from input to output
            foreach (Connector cp in FindVisualChildren<Connector>(MainWindow.GetWindow(this)))
            {
                if (cp.IsConnected && cp.TypeOfInputOutput == InputOutputType.Output)
                {
      
                    var output = cp.TransformToAncestor(MainWindow.GetWindow(this)).Transform(new Point(0, 0));
                    var input = cp.Connection.TransformToAncestor(MainWindow.GetWindow(this)).Transform(new Point(0, 0));

                    input.Y += 5;
                    output.Y += 5;

                    // linear
                    //drawingContext.DrawLine(new Pen(color, thickness), first, second);

                    int connectionStrenght = networkView.BezierStrength;

                    double[] ptind = new double[] { output.X, output.Y,
                                                    output.X + connectionStrenght, output.Y,
                                                    input.X - connectionStrenght, input.Y,
                                                    input.X, input.Y};
                    double[] p = new double[POINTS_ON_CURVE];

                    bezierCurve.Bezier2D(ptind, (POINTS_ON_CURVE) / 2, p);

                    // draw points
                    for (int i = 1; i != POINTS_ON_CURVE - 3; i += 2)
                    {
                        drawingContext.DrawLine(new Pen(color, thickness), new Point((int)p[i + 1], (int)p[i]), new Point((int)p[i + 3], (int)p[i + 2]));
                    }
                }
            }            
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
        {
            int value = mouseWheelEventArgs.Delta;

            double scaleDelta = 0;

            if (value > 0)
            {
                scaleDelta = 0.1;
                if (NodeViewModel.GlobalScaleDelta > 0.5)
                    return;
            }
            else
            {
                scaleDelta = -0.1;
                if (NodeViewModel.GlobalScaleDelta < -0.3)
                    return;
            }

           NodeViewModel.GlobalScaleDelta += scaleDelta;
           gridSize += (int)(scaleDelta * 50);

           if (gridSize < 5)
               gridSize = 5;

            foreach (var nodeViewModel in networkView.Nodes)
            {
                nodeViewModel.ScaleBy(scaleDelta);
            }

        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed && !NodeViewModel.IsNodeDragging)
                DragMouseDown = true;

            if (Connector.CurrentConnection != null)
                Connector.StopConnecting();
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            gridLastPos.X = 0;
            gridLastPos.Y = 0;

            DragMouseDown = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (NodeViewModel.IsNodeDragging || !DragMouseDown)
                return;
            
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                if (gridLastPos.X == 0 && gridLastPos.Y == 0)
                {

                }
                else
                {
                    double x = gridLastPos.X - Mouse.GetPosition(this).X;
                    double y = gridLastPos.Y - Mouse.GetPosition(this).Y; 
                    GridOffset.X -= x;
                    GridOffset.Y -= y;

                }

                gridLastPos.X = Mouse.GetPosition(this).X;
                gridLastPos.Y = Mouse.GetPosition(this).Y;
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
    }

    class MyImage : System.Windows.Controls.Image
    {
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(System.Windows.Media.Brushes.Red, null, new Rect(0, 0, 100, 100));
        }
    }

}


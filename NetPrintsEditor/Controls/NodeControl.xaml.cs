﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NetPrintsEditor.ViewModels;
using System.Collections.Specialized;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for NodeControl.xaml
    /// </summary>
    public partial class NodeControl : UserControl
    {
        public static DependencyProperty NodeProperty = DependencyProperty.Register(
            nameof(NetPrints.Graph.Node), typeof(NodeVM), typeof(NodeControl));

        public NodeVM Node
        {
            get => GetValue(NodeProperty) as NodeVM;
            set => SetValue(NodeProperty, value);
        }

        public NodeControl()
        {
            InitializeComponent();
        }

        private void OnInputExecPinsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
        }

        #region Dragging
        private bool dragging = false;
        private Point dragStartMousePosition;
        private Point dragStartElementPosition;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            dragging = true;

            dragStartElementPosition = new Point(Node.PositionX, Node.PositionY);
            dragStartMousePosition = e.GetPosition(null);

            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (dragging)
            {
                dragging = false;

                ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (dragging)
            {
                Point mousePosition = e.GetPosition(null);

                Vector offset = mousePosition - dragStartMousePosition;
                
                Node.PositionX = dragStartElementPosition.X + offset.X;
                Node.PositionY = dragStartElementPosition.Y + offset.Y;

                Node.PositionX -= Node.PositionX % MethodEditorControl.GridCellSize;
                Node.PositionY -= Node.PositionY % MethodEditorControl.GridCellSize;

                InvalidateVisual();
            }
        }
        #endregion
    }
}

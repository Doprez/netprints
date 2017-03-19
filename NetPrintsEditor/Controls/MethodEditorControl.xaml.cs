﻿using System;
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
using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Translator;
using NetPrintsEditor.Adorners;
using NetPrintsEditor.ViewModels;
using NetPrintsEditor.Commands;
using System.Collections.ObjectModel;
using System.Reflection;
using NetPrints.Extensions;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for FunctionEditorControl.xaml
    /// </summary>
    public partial class MethodEditorControl : UserControl
    {
        public const double GridCellSize = 20;

        private double nodeListScale = 1;
        private const double NodeListMinScale = 0.3;
        private const double NodeListMaxScale = 1.0;
        private const double NodeListScaleFactor = 1.3;

        public MethodVM Method
        {
            get => GetValue(MethodProperty) as MethodVM;
            set => SetValue(MethodProperty, value);
        }

        public static DependencyProperty MethodProperty = DependencyProperty.Register(
            nameof(Method), typeof(MethodVM), typeof(MethodEditorControl));
        
        public static DependencyProperty SuggestionsProperty = DependencyProperty.Register(
            nameof(Suggestions), typeof(ObservableCollection<object>), typeof(MethodEditorControl));

        public ObservableCollection<object> Suggestions
        {
            get => (ObservableCollection<object>)GetValue(SuggestionsProperty);
            set => SetValue(SuggestionsProperty, value);
        }

        public MethodEditorControl()
        {
            InitializeComponent();
        }

        public void ShowVariableGetSet(VariableVM variable, Point position)
        {
            variableGetSet.Visibility = Visibility.Visible;
            variableGetSet.Tag = new object[] { variable, position };

            variableSetButton.Tag = variableGetSet.Tag;
            variableGetButton.Tag = variableGetSet.Tag;
            
            Canvas.SetLeft(variableGetSet, position.X - variableGetSet.Width / 2);
            Canvas.SetTop(variableGetSet, position.Y - variableGetSet.Height / 2);
        }

        public void HideVariableGetSet()
        {
            variableGetSet.Visibility = Visibility.Hidden;
        }

        private void OnVariableSetClicked(object sender, RoutedEventArgs e)
        {
            if(sender is Control c && c.Tag is object[] o && o.Length == 2 && o[0] is VariableVM v && o[1] is Point pos)
            {
                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(VariableSetterNode), Method.Method, pos.X, pos.Y,
                    v.Name, v.VariableType
                ));
            }

            HideVariableGetSet();
        }

        private void OnVariableGetClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Control c && c.Tag is object[] o && o.Length == 2 && o[0] is VariableVM v && o[1] is Point pos)
            {
                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(VariableGetterNode), Method.Method, pos.X, pos.Y,
                    v.Name, v.VariableType
                ));
            }

            HideVariableGetSet();
        }

        private void OnVariableGetSetMouseLeave(object sender, MouseEventArgs e)
        {
            HideVariableGetSet();
        }

        private void OnGridDrop(object sender, DragEventArgs e)
        {
            if (Method != null && e.Data.GetDataPresent(typeof(VariableVM)))
            {
                VariableVM variable = e.Data.GetData(typeof(VariableVM)) as VariableVM;
                
                ShowVariableGetSet(variable, e.GetPosition(variableGetSetCanvas));

                e.Handled = true;
            }
            else if(e.Data.GetDataPresent(typeof(NodePinVM)))
            {
                // Show all relevant methods for the type of the pin if its a data pin

                NodePinVM pin = e.Data.GetData(typeof(NodePinVM)) as NodePinVM;

                if (pin.Pin is NodeDataPin dataPin)
                {
                    if (dataPin is NodeOutputDataPin odp)
                    {
                        Suggestions = new ObservableCollection<object>(
                            ReflectionUtil.GetPublicMethodsForType(odp.PinType));
                    }
                    else if (dataPin is NodeInputDataPin idp)
                    {
                        Suggestions = new ObservableCollection<object>(
                            ReflectionUtil.GetStaticFunctionsWithReturnType(idp.PinType));
                    }

                    // Open the context menu
                    grid.ContextMenu.PlacementTarget = grid;
                    grid.ContextMenu.IsOpen = true;

                    e.Handled = true;
                }
                else if(pin.Pin is NodeOutputExecPin oxp)
                {
                    pin.ConnectedPin = null;
                }
            }
            if (Method != null && e.Data.GetDataPresent(typeof(MethodVM)))
            {
                Point mousePosition = e.GetPosition(methodEditorWindow);
                MethodVM method = e.Data.GetData(typeof(MethodVM)) as MethodVM;

                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(CallMethodNode), Method.Method, mousePosition.X, mousePosition.Y,
                    method.Name, method.ArgumentTypes, method.ReturnTypes
                ));

                e.Handled = true;
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (Method != null && e.Data.GetDataPresent(typeof(VariableVM)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else if(e.Data.GetDataPresent(typeof(NodePinVM)))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
            }
            else if(Method != null && e.Data.GetDataPresent(typeof(MethodVM)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Suggestions = new ObservableCollection<object>(ReflectionUtil.GetStaticFunctions());
            Suggestions.Add(typeof(ForLoopNode));
            Suggestions.Add(typeof(IfElseNode));
        }

        private void OnMouseWheelScroll(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta < 0)
            {
                nodeListScale /= NodeListScaleFactor;
            }
            else
            {
                nodeListScale *= NodeListScaleFactor;
            }

            // Clamp scale between min and max
            if(nodeListScale < NodeListMinScale)
            {
                nodeListScale = NodeListMinScale;
            }
            else if(nodeListScale > NodeListMaxScale)
            {
                nodeListScale = NodeListMaxScale;
            }

            nodeList.LayoutTransform = new ScaleTransform(nodeListScale, nodeListScale);
            e.Handled = true;
        }
    }
}

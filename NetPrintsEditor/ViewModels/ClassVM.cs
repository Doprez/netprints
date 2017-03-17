﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.ComponentModel;
using NetPrints.Core;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NetPrintsEditor.ViewModels
{
    public class ClassVM : INotifyPropertyChanged
    {
        // Wrapped attributes of Class
        public ObservableCollection<Variable> Attributes { get => cls.Attributes; }
        public ObservableCollection<Method> Methods { get => cls.Methods; }

        public Type SuperType
        {
            get => cls.SuperType;
            set
            {
                cls.SuperType = value;
                OnPropertyChanged();
            }
        }

        public string Namespace
        {
            get => cls.Namespace;
            set
            {
                cls.Namespace = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => cls.Name;
            set
            {
                cls.Name = value;
                OnPropertyChanged();
            }
        }

        public ClassModifiers Modifiers
        {
            get => cls.Modifiers;
            set
            {
                cls.Modifiers = value;
                OnPropertyChanged();
            }
        }

        public Class Class
        {
            get => cls;
            set
            {
                if (cls != value)
                {
                    cls = value;
                    OnPropertyChanged();
                }
            }
        }

        private Class cls;

        public ClassVM(Class cls)
        {
            this.cls = cls;
        }

#region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
#endregion
    }
}

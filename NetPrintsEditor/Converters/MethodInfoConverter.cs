﻿using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace NetPrintsEditor.Converters
{
    [ValueConversion(typeof(MethodInfo), typeof(string))]
    public class MethodInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is MethodInfo methodInfo)
            {
                string paramTypeNames = string.Join(", ", 
                    methodInfo.GetParameters().Select(p => $"{p.ParameterType} {p.Name}"));

                string s = $"{methodInfo.DeclaringType} {methodInfo.Name} ({paramTypeNames})";

                if(methodInfo.ReturnType != typeof(void))
                {
                    s += $" : {methodInfo.ReturnType}";
                }

                return s;
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace LyncWPFApplication3
{
    class StringToResourceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement targetObject = values[0] as FrameworkElement;

            if (targetObject == null)
            {
                return DependencyProperty.UnsetValue;
            }
            return targetObject.TryFindResource(values[1]);
        }


        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

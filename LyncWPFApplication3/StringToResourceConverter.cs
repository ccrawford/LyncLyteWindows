using LyncLights;
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

    [ValueConversion(typeof(LIGHTS), typeof(string))]
    public class LightToIconFilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((LIGHTS)value)
            {
                case LIGHTS.RED:
                    return "Icons/ll tube red.png";
                case LIGHTS.YELLOW:
                    return "Icons/ll tube yellow.png";
                case LIGHTS.GREEN:
                    return "Icons/ll tube green.png";
                default:
                    return "Icons/ll tube off.png";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }

    public sealed class GenericBooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public GenericBooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed) { }
    }


}

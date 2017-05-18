using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TdkDictionaryWin10.Converters
{
    class BooleanToVisibilityConverter : IValueConverter
    {
        // Set to the boolean value that will be converted as visible
        private bool referenceValue = true;
        public bool ReferenceValue
        {
            get { return referenceValue; }
            set { referenceValue = value; }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool)value == referenceValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible && referenceValue;
        }
    }
}

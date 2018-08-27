using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CoffeeFlow.Base;

namespace CoffeeFlow.ValueConverters
{
    public class ConnectorTypeToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ConnectorType type = (ConnectorType)value;

            if (type == ConnectorType.ExecutionFlow)
                return new CornerRadius(1, 1, 1, 1);
            else
                return new CornerRadius(8, 8, 8, 8);


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

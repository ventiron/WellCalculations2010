using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace WellCalculations2010.Converter
{
    internal class RowNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CollectionViewSource collectionviewsource = parameter as CollectionViewSource;

            int counter = 1;
            foreach (object item in collectionviewsource.View)
            {
                if (item == value)
                {
                    return counter.ToString();
                }
                counter++;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

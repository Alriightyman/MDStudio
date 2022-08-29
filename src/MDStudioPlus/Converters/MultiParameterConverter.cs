using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MDStudioPlus.Converters
{
	public class MultiParameterConverter : IMultiValueConverter
	{
		/// <summary>
		/// dobbin of the converter
		/// </summary>
		/// <param name="value">command parameters binded by means of multibiniding</param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns>object as List of partial parameters</returns>
		public object Convert(object[] value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return new List<object>(value);
		}

		/// <summary>
		/// here - mandatory duty
		/// </summary>
		public object[] ConvertBack(object value, Type[] targetTypes,
			object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}

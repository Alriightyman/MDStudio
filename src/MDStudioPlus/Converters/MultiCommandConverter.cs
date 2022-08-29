/**/using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MDStudioPlus.Converters
{
	public class MultiCommandConverter : IMultiValueConverter
	{
		private List<object> _value = new List<object>();

		/// <summary>
		/// dobbin of the converter
		/// </summary>
		/// <param name="value">commands binded by means of multibiniding</param>
		/// <returns>compound Relay command</returns>
		public object Convert(object[] value, Type targetType,
			object parameter, CultureInfo culture)
		{
			_value.AddRange(value);
			return new RelayCommand(GetCompoundExecute(), GetCompoundCanExecute());
		}

		/// <summary>
		/// here - mandatory duty
		/// </summary>
		public object[] ConvertBack(object value, Type[] targetTypes,
			object parameter, CultureInfo culture)
		{
			return null;
		}

		/// <summary>
		/// for execution of all commands
		/// </summary>
		/// <returns>Action<object> that plays a role of the joint Execute</returns>
		private Action<object> GetCompoundExecute()
		{
			return (parameter) =>
			{
				var items = ((List<object>)_value).Zip((List<object>)parameter, (v, p) => new { First = v, Second = p });
				foreach (var item in items)
					if (item.First != default(RelayCommand))
						((RelayCommand)item.First).Execute(item.Second);
			};
		}

		/// <summary>
		/// for check if execution of all commands is possible
		/// </summary>
		/// <returns>Predicate<object> that plays a role of the joint CanExecute</returns>
		private Predicate<object> GetCompoundCanExecute()
		{
			return (parameter) =>
			{
				bool res = true;
				var items = ((List<object>)_value).Zip((List<object>)parameter, (v, p) => new { First = v, Second = p });
				foreach (var item in items)
					if (item.First != default(RelayCommand))
						res &= ((RelayCommand)item.First).CanExecute(item.Second);
				return res;
			};
		}
	}
}
 
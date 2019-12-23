using DSIM.Communications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CommandTransmission
{
    class CmdTargets : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source=(ObservableCollection<Target>)value;
            var target = string.Join(",", source.
                Where(i => { return i.IsSelected; }).
                Select(i => { return i.Name; }));
            return target;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

﻿using DSIM.Communications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CommandTransmission
{
    class CmdListRowHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return GridLength.Auto;
            }
            else
            {
                return new GridLength(0.25, GridUnitType.Star);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 这个转换器可以用在两个地方
    class CmdListVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 这个转换器可以用在两个地方
    class CmdCheckVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (bool)value;
            if (source)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class SimpleReverseBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 用于界面Comobox
    class CmdStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (CmdState)value;
            switch (source)
            {
                case CmdState.未缓存:
                    return 0;
                case CmdState.已缓存:
                    return 1;
                case CmdState.已下达:
                    return 2;
                case CmdState.已签收:
                    return 3;
                default:
                    break;
            }
            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var target = (int)value;
            switch (target)
            {
                case 0:
                    return CmdState.未缓存;
                case 1:
                    return CmdState.已缓存;
                case 2:
                    return CmdState.已下达;
                case 3:
                    return CmdState.已签收;
            }
            return -1;
        }
    }

    // 已激活的列控命令
    class ActivatedSpeedCmds : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (ObservableCollection<MsgSpeedCommand>)value;
            var result = source.Where(i => i.SpeedCmdState == SpeedCmdState.已激活);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 已设置的列控命令
    class SettedSpeedCmds : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (ObservableCollection<MsgSpeedCommand>)value;
            var result = source.Where(i => i.SpeedCmdState == SpeedCmdState.已设置);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 已拟定的列控命令
    class SketchSpeedCmds : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (ObservableCollection<MsgSpeedCommand>)value;
            var result = source.Where(i => i.SpeedCmdState == SpeedCmdState.已拟定);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

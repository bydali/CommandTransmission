using DSIM.Communications;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace CommandTransmission
{
    /// <summary>
    /// SpeedLimitedCommand.xaml 的交互逻辑
    /// </summary>
    public partial class NewSpeedWindow : MetroWindow
    {
        private IEventAggregator eventAggregator;
        private bool isCheck;
        private MsgSpeedCommand cmdFromNet;

        public NewSpeedWindow(IEventAggregator eventAggregator,
            MsgSpeedCommand cmd, bool isCheck = false)
        {
            this.eventAggregator = eventAggregator;
            this.isCheck = isCheck;

            MsgSpeedCommand speedCmd = new MsgSpeedCommand() { };
            speedCmd.Targets = new ObservableCollection<Target>();
            using (XmlReader reader = XmlReader.Create("AllTargets.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "target")
                    {
                        speedCmd.Targets.Add(new Target() { Name = reader.GetAttribute("name") });
                    }
                    reader.Read();
                }
            }

            DataContext = speedCmd;

            InitializeComponent();

            if (isCheck)
            {
                GenerateCmdContent(cmd);
            }
        }

        /// <summary>
        /// 生成网络命令的文本
        /// </summary>
        /// <param name="cmd"></param>
        private void GenerateCmdContent(MsgSpeedCommand cmd)
        {
            Height = 650;
            Title = "校验列控命令";
            TB.Visibility = Visibility.Visible;
            cmdFromNet = cmd;

            var s = "";
            s += "命令类型：" + cmd.CmdType + "\n";
            s += "限速原因：" + cmd.Reason + "\n";
            s += "线路：" + cmd.RouteName + "\n";
            s += "公里标：" + "K" + cmd.BeginKMark1 + cmd.BeginKMark2 + " + " +
                cmd.BeginKMark3 + cmd.BeginKMark4 + "-----" + "K" +
                cmd.EndKMark1 + cmd.EndKMark2 + "+" +
                cmd.EndKMark3 + cmd.EndKMark4 + "\n";
            s += "开始时间：" + (cmd.BeginNow ? "立即执行" : cmd.BeginTime) + "\n";
            s += "结束时间：" + (cmd.EndLasting ? "持久有效" : cmd.EndTime) + "\n";
            s += "限速值：" + cmd.LimitValue + "\n";
            s += "取消限速：" + cmd.IsCancel + "\n";

            TB.Text = s;
        }

        /// <summary>
        /// 询问是否开始校验列控命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TryCheckSpeedCmd(object sender, RoutedEventArgs e)
        {
            if (isCheck)
            {
                if (CheckEquality((MsgSpeedCommand)DataContext, cmdFromNet))
                {
                    cmdFromNet.Content = TB.Text;
                    eventAggregator.GetEvent<PassSpeedCommand>().
                       Publish(cmdFromNet);

                    Close();
                }
                else
                {
                    MessageBox.Show("输入与远程消息不一致");
                }
            }
            else
            {
                if (MessageBox.Show("是否开始校验当前列控命令", "操作提示", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                       MessageBoxResult.Yes)
                {
                    eventAggregator.GetEvent<NotifyMain>().
                       Publish((MsgSpeedCommand)DataContext);

                    Close();
                }
            }
        }

        private bool CheckEquality(MsgSpeedCommand local, MsgSpeedCommand net)
        {
            if (local.EndTime != net.EndTime)
            {
                return false;
            }
            if (local.BeginTime != net.BeginTime)
            {
                return false;
            }
            if (local.IsCancel != net.IsCancel)
            {
                return false;
            }
            if (local.LimitValue != net.LimitValue)
            {
                return false;
            }
            if (local.EndKMark4 != net.EndKMark4)
            {
                return false;
            }
            if (local.EndKMark3 != net.EndKMark3)
            {
                return false;
            }
            if (local.EndKMark2 != net.EndKMark2)
            {
                return false;
            }
            if (local.EndKMark1 != net.EndKMark1)
            {
                return false;
            }
            if (local.BeginKMark4 != net.BeginKMark4)
            {
                return false;
            }
            if (local.BeginKMark3 != net.BeginKMark3)
            {
                return false;
            }
            if (local.BeginKMark2 != net.BeginKMark2)
            {
                return false;
            }
            if (local.BeginKMark1 != net.BeginKMark1)
            {
                return false;
            }
            if (local.RouteName != net.RouteName)
            {
                return false;
            }
            if (local.CmdType != net.CmdType)
            {
                return false;
            }
            if (local.Reason != net.Reason)
            {
                return false;
            }
            if (local.EndLasting != net.EndLasting)
            {
                return false;
            }
            return true;
        }
    }
}

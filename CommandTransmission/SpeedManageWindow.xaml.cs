using MahApps.Metro.Controls;
using Microsoft.Practices.Unity;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
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
using YDMSG;
using DSIM.Communications;

namespace CommandTransmission
{
    /// <summary>
    /// SpeedCommandManageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SpeedManageWindow : MetroWindow
    {
        private IEventAggregator eventAggregator;
        private IUnityContainer container;

        public SpeedManageWindow(IEventAggregator eventAggregator,
            IUnityContainer container)
        {
            this.eventAggregator = eventAggregator;
            this.container = container;

            SetItemSource();

            InitializeComponent();
        }

        private void SetItemSource()
        {
            var appVM = container.Resolve<AppVM>();
            DataContext = appVM;
        }

        private void ActivateCmd(object sender, RoutedEventArgs e)
        {
            if (((TabItem)Tab.SelectedItem).Header.ToString() == "已拟定")
            {
                var cmd = SketchDG.SelectedItem;
                if (cmd != null)
                {
                    if (((YDMSG.MsgSpeedCommand)cmd).CmdState == YDMSG.CmdState.已签收)
                    {
                        ((YDMSG.MsgSpeedCommand)cmd).Category = MsgCategoryEnum.CommandActive;
                        ActivateSpeedCmd((YDMSG.MsgSpeedCommand)cmd, "DSIM.Command.Active");
                    }
                    else
                    {
                        MessageBox.Show("命令未被签收");
                    }

                }
            }
        }

        private void ExecuteCmd(object sender, RoutedEventArgs e)
        {
            if (((TabItem)Tab.SelectedItem).Header.ToString() == "已激活")
            {
                var cmd = ActivatedDG.SelectedItem;
                if (cmd != null)
                {
                    if (((YDMSG.MsgSpeedCommand)cmd).CmdState == YDMSG.CmdState.已签收)
                    {
                        ((YDMSG.MsgSpeedCommand)cmd).Category = MsgCategoryEnum.CommandExecute;
                        ActivateSpeedCmd((YDMSG.MsgSpeedCommand)cmd, "DSIM.Command.Execute");
                    }
                    else
                    {
                        MessageBox.Show("命令未被签收");
                    }
                }
            }
        }

        private async void ActivateSpeedCmd(YDMSG.MsgSpeedCommand cmd, string topic)
        {
            await Task.Run(() =>
            {
                IO.SendMsg(cmd, topic);
            });
        }
    }
}

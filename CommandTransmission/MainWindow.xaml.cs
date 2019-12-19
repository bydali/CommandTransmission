using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Prism.Events;
using YDMSG;

namespace CommandTransmission
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private long cmdSN = 1;
        private IEventAggregator eventAggregator;

        public MainWindow(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            this.eventAggregator = eventAggregator;
            RegisterALLEvent();
            IO.ReceiveMsg(eventAggregator);
        }

        private void RegisterALLEvent()
        {
            eventAggregator.GetEvent<EditNewCommand>().Unsubscribe(NewEdittingCmd);
            eventAggregator.GetEvent<EditNewCommand>().Subscribe(NewEdittingCmd);

            eventAggregator.GetEvent<ReceiptCommand>().Unsubscribe(ReceiptCmd);
            eventAggregator.GetEvent<ReceiptCommand>().Subscribe(ReceiptCmd);
        }

        private void ReceiptCmd(MsgReceipt data)
        {
            var sendLst = (ObservableCollection<MsgDispatchCommand>)sendCmdsDg.ItemsSource;
            var sendingLst = (ObservableCollection<MsgDispatchCommand>)sendingCmdsDg.ItemsSource;

            var cmd = sendingLst.Where(i => i.CmdSN == data.CmdSN).First();
            var station = cmd.Targets.Where(i => i.Name == data.Station).First();

            ChangeStationState(station, data);

            if (IsAllTargetChecked(cmd))
            {
                cmd.CmdState = CmdState.全部签收;
                sendingLst.Remove(cmd);
                sendLst.Insert(0, cmd);
            }
            else
            {
                cmd.CmdState = CmdState.部分签收;
            }
        }

        private bool IsAllTargetChecked(MsgDispatchCommand cmd)
        {
            foreach (var item in cmd.Targets.Where(i => i.IsSelected))
            {
                if (!item.IsChecked)
                {
                    return false;
                }
            }
            return true;
        }


        // 新建一个命令
        private void NewEdittingCmd(MsgDispatchCommand data)
        {
            data.CmdSN = "XXX";

            //新命令的上下文数据
            var appVM = (AppVM)DataContext;
            appVM.CurrentCmd = data;

            CmdParagraph.Inlines.Clear();

            var lst = data.Content.ToString().Split(new string[] { "***" }, StringSplitOptions.None);
            for (int i = 0; i < lst.Length; i++)
            {
                Run r = new Run(lst[i]);
                CmdParagraph.Inlines.Add(r);

                if (i != lst.Length - 1)
                {
                    Hyperlink hl = new Hyperlink();
                    hl.Inlines.Add(new Run("        "));
                    CmdParagraph.Inlines.Add(hl);
                }
            }
        }

        private void CreateCommand(object sender, RoutedEventArgs e)
        {
            SpeedLimitedCommandWindow window = new SpeedLimitedCommandWindow();
            window.ShowDialog();
        }

        private void SpeedManage(object sender, RoutedEventArgs e)
        {
            SpeedCommandManageWindow window = new SpeedCommandManageWindow();
            window.ShowDialog();
        }

        private void CmdTemplateClick(object sender, RoutedEventArgs e)
        {
            var dc = (AppVM)DataContext;
            if (dc.CurrentCmd == null)
            {
                ShowCmdTemplateWindow();
            }
            else
            {
                if (dc.CurrentCmd.CmdState != CmdState.编辑)
                {
                    ShowCmdTemplateWindow();
                }
                else
                {
                    SaveAndNewWindow();
                }
            }
        }

        private void SaveAndNewWindow()
        {
            CacheCurrentCmd(null, null);
            ShowCmdTemplateWindow();
        }

        private void ShowCmdTemplateWindow()
        {
            if (Application.Current.Windows.OfType<CommandTemplateWindow>().Count() == 0)
            {
                CommandTemplateWindow window = new CommandTemplateWindow(eventAggregator);
                window.Show();
            }
            else
            {
                var window = Application.Current.Windows.OfType<CommandTemplateWindow>().First();
                window.WindowState = WindowState.Normal;
            }
        }

        private void ChangeEdittingCmd(object sender, MouseButtonEventArgs e)
        {
            var cmd = (MsgDispatchCommand)((DataGridRow)sender).DataContext;

            var cmdCurrent = (MsgDispatchCommand)CmdEdittingGrid.DataContext;
            Inline[] tmp = new Inline[CmdParagraph.Inlines.Count];
            CmdParagraph.Inlines.CopyTo(tmp, 0);
            cmdCurrent.Content = tmp;

            var appVM = (AppVM)DataContext;
            appVM.CurrentCmd = cmd;

            CmdParagraph.Inlines.Clear();
            foreach (var item in (Inline[])cmd.Content)
            {
                CmdParagraph.Inlines.Add(item);
            }
        }

        private string Content2String(Paragraph p)
        {
            var content = "";
            foreach (var item in p.Inlines)
            {
                if (item is Run)
                {
                    content += ((Run)item).Text;
                }
                if (item is Hyperlink)
                {
                    foreach (Run r in ((Hyperlink)item).Inlines)
                    {
                        content += r.Text;
                    }
                }
            }
            return content;
        }

        private async void SendMsg(object sender, RoutedEventArgs e)
        {
            var currentCmd = (MsgDispatchCommand)CmdEdittingGrid.DataContext;
            var cachedLst = (ObservableCollection<MsgDispatchCommand>)cachedCmdsDg.ItemsSource;
            if (currentCmd != null && cachedLst.Contains(currentCmd))
            {
                var cmd = (MsgDispatchCommand)CmdEdittingGrid.DataContext;
                cmd.Content = Content2String(CmdParagraph);

                if (MessageBox.Show("请确认命令内容，是否继续", "操作提示",
                    MessageBoxButton.YesNo, MessageBoxImage.Information)
                    == MessageBoxResult.Yes)
                {
                    var controller = await this.ShowProgressAsync("", "发送中……");
                    controller.SetIndeterminate();

                    try
                    {
                        await Task.Run(() =>
                                        {
                                            IO.SendMsg(cmd);
                                        });

                        await controller.CloseAsync();

                        ChangeCmdState(cmd);

                        var sendingLst = (ObservableCollection<MsgDispatchCommand>)sendingCmdsDg.ItemsSource;
                        cachedLst.Remove(cmd);
                        sendingLst.Insert(0, cmd);
                    }
                    catch (Exception except)
                    {
                        await controller.CloseAsync();
                        MessageBox.Show(except.Message, "下达失败");
                    }
                }

            }
            else
            {
                MessageBox.Show("请将命令加入缓存队列");
            }
        }



        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void CacheCurrentCmd(object sender, RoutedEventArgs e)
        {
            var cmd = (MsgDispatchCommand)CmdEdittingGrid.DataContext;
            Inline[] tmp = new Inline[CmdParagraph.Inlines.Count];
            CmdParagraph.Inlines.CopyTo(tmp, 0);
            cmd.Content = tmp;

            var cachedLst = (ObservableCollection<MsgDispatchCommand>)cachedCmdsDg.ItemsSource;
            if (!cachedLst.Contains(cmd))
            {
                cachedLst.Insert(0, cmd);
            }
            MessageBox.Show("当前命令更新内容已缓存");
        }

        private void DeleteTheCache(object sender, RoutedEventArgs e)
        {
            var cmd = cachedCmdsDg.SelectedItem;
            if (cmd != null)
            {
                ((AppVM)DataContext).CachedCmds.Remove((MsgDispatchCommand)cmd);
            }
        }

        private void DeleteAllCache(object sender, RoutedEventArgs e)
        {
            var appVM = (AppVM)DataContext;
            appVM.CachedCmds.Clear();
        }

        private void ChangeCmdState(MsgDispatchCommand cmd)
        {
            cmd.CmdState = CmdState.已下达;
        }

        private void ChangeStationState(Cmd2Station station, MsgReceipt data)
        {
            station.IsChecked = true;
            station.CheckTime = data.CheckTime;
            station.Checkee = data.Checkee;
        }
    }
}

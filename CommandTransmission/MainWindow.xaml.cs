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
        private int cmdSN;
        private IEventAggregator eventAggregator;

        public MainWindow(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            InitializeComponent();

            SetCommandBindings();
            RegisterALLEvent();
            IO.ReceiveMsg(eventAggregator);
        }

        private void SetCommandBindings()
        {
            var appVM = (AppVM)DataContext;
            CommandBindings.Add(
                new CommandBinding(
                    appVM.CacheCmd,
                    CacheExecute,
                    appVM.CacheCanExecute));
            CommandBindings.Add(
                new CommandBinding(
                    appVM.ApplyFor,
                    ApplyForExecute,
                    appVM.ApplyForCanExecute));
            CommandBindings.Add(
                new CommandBinding(
                    appVM.SendCmd,
                    appVM.SendCmdExecute,
                    appVM.SendCmdCanExecute));
            CommandBindings.Add(
                new CommandBinding(
                    appVM.AgentSign,
                    appVM.AgentSignExecute,
                    appVM.AgentSignCanExecute));
        }

        private void ApplyForExecute(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private async void CacheExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var cmd = ((AppVM)DataContext).CurrentCmd;
            cmd.Content = FillCmdContent(cmd);

            try
            {
                await Task.Run(() =>
                {
                    IO.SendMsg(cmd, "DSIM.Command.Create");
                });
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message);
            }
        }

        private string FillCmdContent(MsgDispatchCommand cmd)
        {
            string s = "";

            foreach (var item in CmdParagraph.Inlines)
            {
                if (item is Run)
                {
                    s += ((Run)item).Text;
                }
                else
                {
                    foreach (Run fill in ((Hyperlink)item).Inlines)
                    {
                        s += fill.Text;
                    }
                }
            }

            return s;
        }

        private void RegisterALLEvent()
        {
            eventAggregator.GetEvent<EditNewCommand>().Unsubscribe(NewEdittingCmd);
            eventAggregator.GetEvent<EditNewCommand>().Subscribe(NewEdittingCmd);

            eventAggregator.GetEvent<ReceiptCommand>().Unsubscribe(ReceiptCmd);
            eventAggregator.GetEvent<ReceiptCommand>().Subscribe(ReceiptCmd);

            eventAggregator.GetEvent<CacheCommand>().Unsubscribe(UpdateCacheCmd);
            eventAggregator.GetEvent<CacheCommand>().Subscribe(UpdateCacheCmd, ThreadOption.UIThread);
        }

        /// <summary>
        /// 用于更新网络广播到本地的缓存命令
        /// </summary>
        /// <param name="cmd"></param>
        private void UpdateCacheCmd(MsgDispatchCommand cmd)
        {
            var result = ((AppVM)DataContext).CachedCmds.Where(i => i.CmdSN == cmd.CmdSN);
            if (result.Count() != 0)
            {
                if (((AppVM)DataContext).CachedCmds.Count != 0)
                {
                    ((AppVM)DataContext).CachedCmds.Remove(result.First());
                }
            }

            ((AppVM)DataContext).CachedCmds.Insert(0, cmd);
            ((AppVM)DataContext).CurrentCmd = cmd;
            cmd.CmdState = CmdState.已缓存;

            CmdParagraph.Inlines.Clear();
            CmdParagraph.Inlines.Add(new Run(cmd.Content.ToString()));
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
                if (!item.IsSigned)
                {
                    return false;
                }
            }
            return true;
        }


        // 新建一个命令
        private void NewEdittingCmd(MsgDispatchCommand data)
        {
            cmdSN = ((AppVM)DataContext).CachedCmds.Count + 1;
            data.CmdSN = cmdSN.ToString();

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

        /// <summary>
        /// 新建命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmdTemplateClick(object sender, RoutedEventArgs e)
        {
            TryCmdTemplateWindow();
        }

        private void SaveAndNewWindow()
        {
            TryCmdTemplateWindow();
        }

        private void TryCmdTemplateWindow()
        {
            var appVM = (AppVM)DataContext;
            if (appVM.CurrentCmd != null)
            {
                // 如果没有缓存，或者缓存了没有新的修改，就直接打开窗口
                if (!appVM.CurrentCmd.IsCached ||
                    (appVM.CurrentCmd.IsCached &&
                    !appVM.CurrentCmd.IsRead2Update))
                {
                    ShowCmdTemplateWindow();
                }
                else
                {
                    if (MessageBox.Show("当前命令已被修改，是否缓存", "操作提示", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                        MessageBoxResult.Yes)
                    {
                        CacheExecute(null, null);
                    }
                    else
                    {
                        appVM.CurrentCmd.IsRead2Update = false;
                        ShowCmdTemplateWindow();
                    }

                }
            }
            else
            {
                ShowCmdTemplateWindow();
            }
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

            if (((AppVM)DataContext).CurrentCmd.IsCached &&
                ((AppVM)DataContext).CurrentCmd.IsRead2Update)
            {
                if (MessageBox.Show("当前命令已被修改，是否缓存", "操作提示", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                    MessageBoxResult.Yes)
                {
                    CacheExecute(null, null);
                }
                else
                {
                    ((AppVM)DataContext).CurrentCmd.IsRead2Update = false;
                }
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

        private void ChangeStationState(Target target, MsgReceipt data)
        {
            target.IsSigned = true;
            target.CheckTime = data.CheckTime;
            target.Checkee = data.Checkee;
        }

        private void CmdTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var appVM = (AppVM)DataContext;
            if (appVM.CurrentCmd != null && appVM.CurrentCmd.IsCached)
            {
                appVM.CurrentCmd.IsRead2Update = true;
            }
        }
    }
}

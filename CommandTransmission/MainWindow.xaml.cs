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
                    SendCmdExecute,
                    appVM.SendCmdCanExecute));
            CommandBindings.Add(
                new CommandBinding(
                    appVM.AgentSign,
                    AgentSignExecute,
                    appVM.AgentSignCanExecute));
        }

        /// <summary>
        /// 代签下达的命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AgentSignExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var cmd = ((AppVM)DataContext).CurrentCmd;
            var agentTarget = (Target)((DataGridCell)e.OriginalSource).DataContext;

            MsgSign check = new MsgSign(cmd.CmdSN, DateTime.Now.ToString(),
                ConfigurationManager.ConnectionStrings["ClientName"].ConnectionString,
                ConfigurationManager.ConnectionStrings["User"].ConnectionString)
            {
                IsAgentSign = true,
                AgentTarget = agentTarget.Name
            };

            try
            {
                await Task.Run(() =>
                {
                    IO.SendMsg(check, "DSIM.Command.AgentSign");
                });
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message);
            }
        }

        /// <summary>
        /// 点击下达按钮触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCmdExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SendCmd("DSIM.Command.Transmit");
        }

        /// <summary>
        /// 点击申请批准按钮触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyForExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (((AppVM)DataContext).CurrentCmd.IsRead2Update)
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
            else
            {
                SendCmd("DSIM.Command.Approve");
            }
        }

        /// <summary>
        /// 点击缓存按钮触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CacheExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var cmd = ((AppVM)DataContext).CurrentCmd;
            if (cmd.IsCached)
            {
                SendCmd("DSIM.Command.Update");
            }
            else
            {
                SendCmd("DSIM.Command.Create");
            }
        }

        private async void SendCmd(string topic)
        {
            var cmd = ((AppVM)DataContext).CurrentCmd;
            cmd.Content = FillCmdContent();

            try
            {
                await Task.Run(() =>
                {
                    IO.SendMsg(cmd, topic);
                });
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message);
            }
        }

        /// <summary>
        /// 将当前命令的内容部分全部转为字符串
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private string FillCmdContent()
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

            eventAggregator.GetEvent<CacheCommand>().Unsubscribe(UpdateCacheCmd);
            eventAggregator.GetEvent<CacheCommand>().Subscribe(UpdateCacheCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<ApproveCommand>().Unsubscribe(ApproveCmd);
            eventAggregator.GetEvent<ApproveCommand>().Subscribe(ApproveCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<TransmitCommand>().Unsubscribe(TransmitCmd);
            eventAggregator.GetEvent<TransmitCommand>().Subscribe(TransmitCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<SignCommand>().Unsubscribe(ReceiptCmd);
            eventAggregator.GetEvent<SignCommand>().Subscribe(ReceiptCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<AgentSignCommand>().Unsubscribe(ReceiptCmd);
            eventAggregator.GetEvent<AgentSignCommand>().Subscribe(ReceiptCmd, ThreadOption.UIThread);
        }

        private void TransmitCmd(MsgDispatchCommand cmd)
        {
            var result = ((AppVM)DataContext).CachedCmds.Where(i => i.CmdSN == cmd.CmdSN);
            ((AppVM)DataContext).CachedCmds.Remove(result.First());

            ((AppVM)DataContext).SendingCmds.Insert(0, cmd);
            ((AppVM)DataContext).CurrentCmd = cmd;

            CmdParagraph.Inlines.Clear();
            CmdParagraph.Inlines.Add(new Run(cmd.Content.ToString()));

            cmd.CmdState = CmdState.已下达;
        }

        /// <summary>
        /// 接收正在申请批准的命令
        /// </summary>
        /// <param name="cmd"></param>
        private void ApproveCmd(MsgDispatchCommand cmd)
        {
            // 这里客户端的用户应该是值班主任
            var user = ConfigurationManager.ConnectionStrings["User"].ConnectionString;
            var result = cmd.Targets.Where(i => i.Name == user);
            if (result.Count() != 0)
            {
                var oldCmd = ((AppVM)DataContext).CachedCmds.Where(i => i.CmdSN == cmd.CmdSN).First();
                ((AppVM)DataContext).CachedCmds.Remove(oldCmd);
                ((AppVM)DataContext).CachedCmds.Insert(0, cmd);
                ((AppVM)DataContext).CurrentCmd = cmd;

                if (MessageBox.Show("当前命令正在申请批准，请选择是否批准", "操作提示", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                        MessageBoxResult.Yes)
                {
                    cmd.Approve();
                    CacheExecute(null, null);
                }
            }
        }

        /// <summary>
        /// 用于更新网络广播到本地的缓存命令(包含第一次创建)
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

            CmdParagraph.Inlines.Clear();
            CmdParagraph.Inlines.Add(new Run(cmd.Content.ToString()));

            cmd.CmdState = CmdState.已缓存;
        }

        private void ReceiptCmd(MsgSign data)
        {
            var cmd = ((AppVM)DataContext).SendingCmds.Where(i => i.CmdSN == data.CmdSN).First();
            cmd.OneTargetSigned(data);

            if (cmd.CmdState == CmdState.已签收)
            {
                ((AppVM)DataContext).SendingCmds.Remove(cmd);
                ((AppVM)DataContext).SendCmds.Insert(0, cmd);
            }

            ((AppVM)DataContext).ReceivedCmds.Insert(0, data);
        }

        // 新建一个命令
        private void NewEdittingCmd(MsgDispatchCommand data)
        {
            int count = ((AppVM)DataContext).CachedCmds.Count +
                ((AppVM)DataContext).SendingCmds.Count +
                ((AppVM)DataContext).SendCmds.Count;

            data.CmdSN = (count + 1).ToString();

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
                if (appVM.CurrentCmd.CmdState != CmdState.已缓存)
                {
                    ShowCmdTemplateWindow();
                }
                else
                {
                    // 如果没有缓存，或者缓存了没有新的修改，就直接打开窗口
                    if ((appVM.CurrentCmd.IsCached &&
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

            if (cmd.CmdState != CmdState.已缓存)
            {
                ((AppVM)DataContext).CurrentCmd = cmd;

                CmdParagraph.Inlines.Clear();
                CmdParagraph.Inlines.Add(new Run(cmd.Content.ToString()));
            }
            else
            {
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
                else
                {
                    ((AppVM)DataContext).CurrentCmd = cmd;

                    CmdParagraph.Inlines.Clear();
                    CmdParagraph.Inlines.Add(new Run(cmd.Content.ToString()));
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

        private void ChangeStationState(Target target, MsgSign data)
        {
            target.IsSigned = true;
            target.CheckTime = data.CheckTime;
            target.Signee = data.Signee;
        }

        private void AgentSign(object sender, RoutedEventArgs e)
        {

        }

        private void ContextMenuOpened(object sender, RoutedEventArgs e)
        {

        }
    }
}

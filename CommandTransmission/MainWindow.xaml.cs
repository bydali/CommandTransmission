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
using Microsoft.Practices.Unity;
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
        private IUnityContainer container;

        public MainWindow(IEventAggregator eventAggregator,
           IUnityContainer container)
        {
            this.eventAggregator = eventAggregator;
            this.container = container;

            InitializeComponent();

            SetCommandBindings();
            RegisterALLEvent();

            IO.ReceiveMsg(eventAggregator);
        }

        /// <summary>
        /// 为界面命令管理按钮设置Command
        /// </summary>
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

            eventAggregator.GetEvent<SignCommand>().Unsubscribe(TargetSignCmd);
            eventAggregator.GetEvent<SignCommand>().Subscribe(TargetSignCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<AgentSignCommand>().Unsubscribe(TargetSignCmd);
            eventAggregator.GetEvent<AgentSignCommand>().Subscribe(TargetSignCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<NotifyMain>().Unsubscribe(SendSpeedCmd);
            eventAggregator.GetEvent<NotifyMain>().Subscribe(SendSpeedCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<CheckSpeedCommand>().Unsubscribe(CheckSpeedCmd);
            eventAggregator.GetEvent<CheckSpeedCommand>().Subscribe(CheckSpeedCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<PassSpeedCommand>().Unsubscribe(PassSpeedCmd);
            eventAggregator.GetEvent<PassSpeedCommand>().Subscribe(PassSpeedCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<CacheSpeedCommand>().Unsubscribe(CacheSpeedCmd);
            eventAggregator.GetEvent<CacheSpeedCommand>().Subscribe(CacheSpeedCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<ActiveSpeedCommand>().Unsubscribe(ActiveSpeedCmd);
            eventAggregator.GetEvent<ActiveSpeedCommand>().Subscribe(ActiveSpeedCmd, ThreadOption.UIThread);

            eventAggregator.GetEvent<ExecuteSpeedCommand>().Unsubscribe(ExecuteSpeedCmd);
            eventAggregator.GetEvent<ExecuteSpeedCommand>().Subscribe(ExecuteSpeedCmd, ThreadOption.UIThread);
        }

        private void ExecuteSpeedCmd(MsgSpeedCommand cmd)
        {
            var result = ((AppVM)DataContext).SpeedCmds.Where(i => i.CmdSN == cmd.CmdSN);
            if (result.Count() != 0)
            {
                result.First().SpeedCmdState = SpeedCmdState.已设置;
                ((AppVM)DataContext).SpeedCmds = ((AppVM)DataContext).SpeedCmds;
            }
        }

        private void ActiveSpeedCmd(MsgSpeedCommand cmd)
        {
            var result = ((AppVM)DataContext).SpeedCmds.Where(i => i.CmdSN == cmd.CmdSN);
            if (result.Count() != 0)
            {
                result.First().SpeedCmdState = SpeedCmdState.已激活;
                ((AppVM)DataContext).SpeedCmds = ((AppVM)DataContext).SpeedCmds;
            }
        }

        /// <summary>
        /// 收到广播到本地的列控命令，加入缓存
        /// </summary>
        /// <param name="cmd"></param>
        private void CacheSpeedCmd(MsgSpeedCommand cmd)
        {
            UpdateCacheCmd(cmd);

            cmd.SpeedCmdState = SpeedCmdState.已拟定;
            ((AppVM)DataContext).SpeedCmds.Add(cmd);
            ((AppVM)DataContext).SpeedCmds = ((AppVM)DataContext).SpeedCmds;
        }

        /// <summary>
        /// 通过列控校验，并开始向网络广播
        /// </summary>
        /// <param name="cmd"></param>
        private void PassSpeedCmd(MsgSpeedCommand cmd)
        {
            int count = ((AppVM)DataContext).CachedCmds.Count +
                ((AppVM)DataContext).SendingCmds.Count +
                ((AppVM)DataContext).SendCmds.Count;

            cmd.CmdSN = (count + 1).ToString();
            cmd.Title = "列控指令";

            SendCmd("DSIM.Command.SpeedCache", cmd);

            MessageBox.Show("完成校验,已缓存");
        }

        /// <summary>
        /// 校验广播到本地的列控命令
        /// </summary>
        /// <param name="cmd"></param>
        private void CheckSpeedCmd(MsgSpeedCommand cmd)
        {
            // 非此命令编辑用户才能校验
            if (cmd.User != ConfigurationManager.ConnectionStrings["User"].ConnectionString)
            {
                NewSpeedWindow window = new NewSpeedWindow(eventAggregator, cmd, true);
                window.Show();
            }
        }

        /// <summary>
        /// 广播列控命令等待校验
        /// </summary>
        /// <param name="cmd"></param>
        private void SendSpeedCmd(MsgSpeedCommand cmd)
        {
            SendCmd("DSIM.Command.Check", cmd);
        }

        /// <summary>
        /// 尝试打开命令模板窗口
        /// </summary>
        private void TryCmdTemplateWindow(object sender, RoutedEventArgs e)
        {
            var appVM = (AppVM)DataContext;
            if (appVM.CurrentCmd != null)
            {
                // 当前命令为除缓存外的其他状态，直接打开模板窗口
                if (appVM.CurrentCmd.CmdState != CmdState.已缓存)
                {
                    ShowCmdTemplateWindow();
                }
                else
                {
                    // 缓存了没有新的修改，直接打开模板窗口
                    if ((appVM.CurrentCmd.CmdState == CmdState.已缓存 &&
                        !appVM.CurrentCmd.IsRead2Update))
                    {
                        ShowCmdTemplateWindow();
                    }
                    // 缓存了有新的修改
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
            // 当前命令为空，直接打开模板窗口
            else
            {
                ShowCmdTemplateWindow();
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
            if (cmd.CmdState == CmdState.已缓存)
            {
                SendCmd("DSIM.Command.Update", ((AppVM)DataContext).CurrentCmd);
            }
            else
            {
                SendCmd("DSIM.Command.Create", ((AppVM)DataContext).CurrentCmd);
            }
        }

        /// <summary>
        /// 点击申请批准按钮触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyForExecute(object sender, ExecutedRoutedEventArgs e)
        {
            // 当前命令已被修改过
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
                SendCmd("DSIM.Command.Approve", ((AppVM)DataContext).CurrentCmd);
            }
        }

        /// <summary>
        /// 点击下达按钮触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCmdExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SendCmd("DSIM.Command.Transmit", ((AppVM)DataContext).CurrentCmd);
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

            MsgCommandSign check = new MsgCommandSign(cmd.CmdSN, DateTime.Now.ToString(),
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
        /// 按主题发送命令
        /// </summary>
        /// <param name="topic"></param>
        private async void SendCmd(string topic, object cmd)
        {
            if (!(cmd is MsgSpeedCommand))
            {
                ((MsgDispatchCommand)cmd).Content = FillCmdContent();
            }

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
        /// 将当前命令的内容部分全部转为字符串（暂时没有找到流文档的序列化方式）
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

        /// <summary>
        /// 新建一个命令到编辑界面
        /// </summary>
        /// <param name="data"></param>
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

        /// <summary>
        /// 处理网络广播到本地的缓存命令(包含第一次创建)
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

        /// <summary>
        /// 处理网络广播到本地的申请消息
        /// </summary>
        /// <param name="cmd"></param>
        private void ApproveCmd(MsgDispatchCommand cmd)
        {
            var user = ConfigurationManager.ConnectionStrings["User"].ConnectionString;
            var result = cmd.Targets.Where(i => i.Name == user);
            // 这里客户端的用户应该是值班主任
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
        /// 处理网络广播到本地的下达命令
        /// </summary>
        /// <param name="cmd"></param>
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
        /// 处理网络广播到本地的签收回执
        /// </summary>
        /// <param name="data"></param>
        private void TargetSignCmd(MsgCommandSign data)
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

        /// <summary>
        /// 打开命令模板窗口
        /// </summary>
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

        /// <summary>
        /// 尝试双击切换命令界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TryChangeCurrentCmd(object sender, MouseButtonEventArgs e)
        {
            var cmd = (MsgDispatchCommand)((DataGridRow)sender).DataContext;

            // 若不是缓存状态，则直接切换
            if (cmd.CmdState != CmdState.已缓存)
            {
                SetCurrentCmd(cmd);
            }
            else
            {
                // 为缓存状态且已被修改
                if (((AppVM)DataContext).CurrentCmd.CmdState == CmdState.已缓存 &&
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
                    SetCurrentCmd(cmd);
                }
            }
        }

        /// <summary>
        /// 设置当前命令为指定值
        /// </summary>
        /// <param name="cmd"></param>
        private void SetCurrentCmd(MsgDispatchCommand cmd)
        {
            ((AppVM)DataContext).CurrentCmd = cmd;

            CmdParagraph.Inlines.Clear();
            CmdParagraph.Inlines.Add(new Run(cmd.Content.ToString()));
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void CreateCommand(object sender, RoutedEventArgs e)
        {
            NewSpeedWindow window = new NewSpeedWindow(eventAggregator, null);
            window.ShowDialog();
        }

        private void SpeedManage(object sender, RoutedEventArgs e)
        {
            SpeedManageWindow window = container.Resolve<SpeedManageWindow>();
            window.ShowDialog();
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            if (allStationDg.ItemsSource != null)
            {
                foreach (var item in (ObservableCollection<Target>)allStationDg.ItemsSource)
                {
                    item.IsSelected = true;
                }
            }
        }

        private void UnSelectAll(object sender, RoutedEventArgs e)
        {
            if (allStationDg.ItemsSource != null)
            {
                foreach (var item in (ObservableCollection<Target>)allStationDg.ItemsSource)
                {
                    item.IsSelected = false;
                }
            }
        }
    }
}

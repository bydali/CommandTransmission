﻿using System;
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
using DSIM.Communications;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace CommandTransmission
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private IEventAggregator eventAggregator;
        private IUnityContainer container;

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// 将当前命令的内容全部转为字符串（暂时没有找到流文档的序列化方式）
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
                        s += fill.Text.Trim();
                    }
                }
            }

            return s;
        }

        public MainWindow(IEventAggregator eventAggregator,
           IUnityContainer container)
        {
            this.eventAggregator = eventAggregator;
            this.container = container;

            InitializeComponent();

            RegisterLocalEvent();
            RegisterMQIO();

            //2020.9.11
            IO.ReceiveMsg(eventAggregator);
        }

        /// <summary>
        /// 注册程序内部事件
        /// </summary>
        private void RegisterLocalEvent()
        {
            var appVM = (AppVM)DataContext;

            eventAggregator.GetEvent<EditNewCommand>().Unsubscribe(NewEdittingCmd);
            eventAggregator.GetEvent<EditNewCommand>().Subscribe(NewEdittingCmd);
        }

        /// <summary>
        /// 从模板窗口传递一个命令副本到主窗口编辑界面
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
        /// 注册网络IO事件对
        /// </summary>
        private void RegisterMQIO()
        {
            var appVM = (AppVM)DataContext;

            // 缓存的发与收
            CommandBindings.Add(
                new CommandBinding(
                    appVM.CacheCmd,
                    CacheExecute,
                    appVM.CacheCanExecute));
            eventAggregator.GetEvent<CacheCommand>().Unsubscribe(UpdateCacheCmd);
            eventAggregator.GetEvent<CacheCommand>().Subscribe(UpdateCacheCmd, ThreadOption.UIThread);

            // 申请的发与收
            CommandBindings.Add(
               new CommandBinding(
                   appVM.ApplyFor,
                   ApplyForExecute,
                   appVM.ApplyForCanExecute));
            eventAggregator.GetEvent<ApproveCommand>().Unsubscribe(ApproveCmd);
            eventAggregator.GetEvent<ApproveCommand>().Subscribe(ApproveCmd, ThreadOption.UIThread);

            // 下达的发与收
            CommandBindings.Add(
               new CommandBinding(
                   appVM.SendCmd,
                   SendCmdExecute,
                   appVM.SendCmdCanExecute));
            eventAggregator.GetEvent<TransmitCommand>().Unsubscribe(TransmitCmd);
            eventAggregator.GetEvent<TransmitCommand>().Subscribe(TransmitCmd, ThreadOption.UIThread);

            // 代签的发与收
            CommandBindings.Add(
                new CommandBinding(
                    appVM.AgentSign,
                    AgentSignExecute,
                    appVM.AgentSignCanExecute));
            eventAggregator.GetEvent<AgentSignCommand>().Unsubscribe(TargetSignCmd);
            eventAggregator.GetEvent<AgentSignCommand>().Subscribe(TargetSignCmd, ThreadOption.UIThread);

            // 收到车站签收
            eventAggregator.GetEvent<SignCommand>().Unsubscribe(TargetSignCmd);
            eventAggregator.GetEvent<SignCommand>().Subscribe(TargetSignCmd, ThreadOption.UIThread);

            // 列控的发起与校验
            eventAggregator.GetEvent<NotifyMain>().Unsubscribe(SendSpeedCmd);
            eventAggregator.GetEvent<NotifyMain>().Subscribe(SendSpeedCmd, ThreadOption.UIThread);
            eventAggregator.GetEvent<CheckSpeedCommand>().Unsubscribe(CheckSpeedCmd);
            eventAggregator.GetEvent<CheckSpeedCommand>().Subscribe(CheckSpeedCmd, ThreadOption.UIThread);

            // 列控通过的广播与收到后的缓存
            eventAggregator.GetEvent<PassSpeedCommand>().Unsubscribe(PassSpeedCmd);
            eventAggregator.GetEvent<PassSpeedCommand>().Subscribe(PassSpeedCmd, ThreadOption.UIThread);
            eventAggregator.GetEvent<CacheSpeedCommand>().Unsubscribe(CacheSpeedCmd);
            eventAggregator.GetEvent<CacheSpeedCommand>().Subscribe(CacheSpeedCmd, ThreadOption.UIThread);

            // 收到列控的激活
            eventAggregator.GetEvent<ActiveSpeedCommand>().Unsubscribe(ActiveSpeedCmd);
            eventAggregator.GetEvent<ActiveSpeedCommand>().Subscribe(ActiveSpeedCmd, ThreadOption.UIThread);

            // 收到列控的执行
            eventAggregator.GetEvent<ExecuteSpeedCommand>().Unsubscribe(ExecuteSpeedCmd);
            eventAggregator.GetEvent<ExecuteSpeedCommand>().Subscribe(ExecuteSpeedCmd, ThreadOption.UIThread);
        }

        /// <summary>
        /// 缓存按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CacheExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var cmd = ((AppVM)DataContext).CurrentCmd;
            ((AppVM)DataContext).CurrentCmd.Category = MsgCategoryEnum.CommandUpdate;
            SendCmd("DSIM.Command.Update", ((AppVM)DataContext).CurrentCmd);
        }

        /// <summary>
        /// 处理网络广播到本地的缓存命令
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
        /// 申请批准按钮事件
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
                ((AppVM)DataContext).CurrentCmd.IsApproving = true;
                ((AppVM)DataContext).CurrentCmd.Category = MsgCategoryEnum.CommandApprove;
                SendCmd("DSIM.Command.Approve", ((AppVM)DataContext).CurrentCmd);
            }
        }

        /// <summary>
        /// 处理网络广播到本地的申请消息
        /// </summary>
        /// <param name="cmd"></param>
        private void ApproveCmd(MsgDispatchCommand cmd)
        {
            var user = ConfigurationManager.ConnectionStrings["User"].ConnectionString;
            var result = cmd.Targets.Where(i => i.Name == user);

            // 这里客户端的用户应该是"值班主任"
            if (result.Count() != 0)
            {
                var oldCmd = ((AppVM)DataContext).CachedCmds.Where(i => i.CmdSN == cmd.CmdSN).First();
                ((AppVM)DataContext).CachedCmds.Remove(oldCmd);
                ((AppVM)DataContext).CachedCmds.Insert(0, cmd);
                ((AppVM)DataContext).CurrentCmd = cmd;

                if (MessageBox.Show("当前命令正在申请批准，请审阅并选择是否批准", "操作提示", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                        MessageBoxResult.Yes)
                {
                    cmd.Approve();
                    CacheExecute(null, null);
                }
            }
        }

        /// <summary>
        /// 点击下达按钮触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCmdExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ((AppVM)DataContext).CurrentCmd.Category = MsgCategoryEnum.CommandTransmit;
            SendCmd("DSIM.Command.Transmit", ((AppVM)DataContext).CurrentCmd);
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
        /// 代签下达的命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AgentSignExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var cmd = ((AppVM)DataContext).CurrentCmd;
            if (cmd.IsApproving)
            {
                var oldCmd = ((AppVM)DataContext).CachedCmds.Where(i => i.CmdSN == cmd.CmdSN).First();
                ((AppVM)DataContext).CachedCmds.Remove(oldCmd);
                ((AppVM)DataContext).CachedCmds.Insert(0, cmd);
                ((AppVM)DataContext).CurrentCmd = cmd;

                cmd.Approve(true);
                CacheExecute(null, null);
            }
            else
            {
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
                        check.Category = MsgCategoryEnum.CommandAgentSign;
                        IO.SendMsg(check, "DSIM.Command.AgentSign");
                    });
                }
                catch (Exception except)
                {
                    MessageBox.Show(except.Message);
                }
            }
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

            var speed = ((AppVM)DataContext).SpeedCmds.Where(i => i.CmdSN == data.CmdSN);
            if (speed.Count() != 0)
            {
                if (cmd.CmdState == CmdState.已签收)
                {
                    speed.First().CmdState = CmdState.已签收;
                }
            }
        }

        /// <summary>
        /// 广播列控命令等待校验
        /// </summary>
        /// <param name="cmd"></param>
        private void SendSpeedCmd(MsgSpeedCommand cmd)
        {
            cmd.Category = MsgCategoryEnum.CommandCheck;
            SendCmd("DSIM.Command.Check", cmd);
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

            cmd.Category = MsgCategoryEnum.SpeedCache;
            SendCmd("DSIM.Command.SpeedCache", cmd);

            MessageBox.Show("完成校验,已缓存");
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
        /// 收到广播到本地的列控命令，置状态为已激活
        /// </summary>
        /// <param name="cmd"></param>
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
        /// 收到广播到本地的列控命令，置状态为已设置
        /// </summary>
        /// <param name="cmd"></param>
        private void ExecuteSpeedCmd(MsgSpeedCommand cmd)
        {
            var result = ((AppVM)DataContext).SpeedCmds.Where(i => i.CmdSN == cmd.CmdSN);
            if (result.Count() != 0)
            {
                result.First().SpeedCmdState = SpeedCmdState.已设置;
                ((AppVM)DataContext).SpeedCmds = ((AppVM)DataContext).SpeedCmds;
            }
        }

        #region 界面按钮事件

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

        /// <summary>
        /// 全选为受令单位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 取消全选为受令单位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 新建列控命令窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateCommand(object sender, RoutedEventArgs e)
        {
            NewSpeedWindow window = new NewSpeedWindow(eventAggregator, null);
            window.ShowDialog();
        }

        /// <summary>
        /// 打开列控管理窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpeedManage(object sender, RoutedEventArgs e)
        {
            SpeedManageWindow window = container.Resolve<SpeedManageWindow>();
            window.ShowDialog();
        }

        #endregion

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
                MessageBox.Show("调度命令已下达");
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message);
            }
        }

        private byte[] intToBytes(int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)(value & 0xFF);
            src[2] = (byte)((value >> 8) & 0xFF);
            src[1] = (byte)((value >> 16) & 0xFF);
            src[0] = (byte)((value >> 24) & 0xFF);
            return src;
        }

        //2020.9.11
        private async void SendCmd2Train(object sender, RoutedEventArgs e)
        {
            if (((AppVM)DataContext).CurrentCmd != null)
            {
                var cmd = ((AppVM)DataContext).CurrentCmd;
                List<string> targets = new List<string>();
                using (StreamReader sr = new StreamReader("cfg.txt"))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        targets.Add(line);
                    }
                }
                string content = FillCmdContent();
                string message = DateTime.Now.ToString() + '\n' +
                    cmd.Title.Split('、')[1] + '\n' +
                    content;
                await Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < targets.Count; i++)
                        {
                            var ip = targets[i].Split('：')[1];
                            var port = int.Parse(targets[i + 1].Split('：')[1]);
                            UdpClient udpclient = new UdpClient();
                            IPEndPoint ipendpoint = new IPEndPoint(IPAddress.Parse(ip), port);

                            byte[] data = Encoding.UTF8.GetBytes(message);
                            byte[] type = intToBytes(2);
                            var tmp = BitConverter.IsLittleEndian;
                            udpclient.Send(type, type.Length, ipendpoint);
                            udpclient.Send(data, data.Length, ipendpoint);
                            udpclient.Close();
                            i++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("发送至列车失败");
                    }
                });
            }
        }
    }
}

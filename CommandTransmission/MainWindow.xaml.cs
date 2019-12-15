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
        private ObservableCollection<MsgYDCommand> CachedCmds;
        private ObservableCollection<MsgYDCommand> SendCmds;
        private ObservableCollection<MsgYDCommand> ReceivedCmds;
        private ObservableCollection<MsgYDCommand> SendingCmds;

        public MainWindow(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            this.eventAggregator = eventAggregator;
            InitialData();
            RegisterALLEvent();
            IO.ReceiveMsg(eventAggregator);
        }

        private void InitialData()
        {
            Title = ConfigurationManager.ConnectionStrings["ClientName"].ConnectionString;

            SendingCmds = new ObservableCollection<MsgYDCommand>();
            sendingCmdsDg.ItemsSource = SendingCmds;

            // 初始化缓存命令
            CachedCmds = new ObservableCollection<MsgYDCommand>();
            using (XmlReader reader = XmlReader.Create("CachedCmds.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd")
                    {
                        //var targets = reader.GetAttribute("targets").Split('\t');
                        //string s = string.Join(",", targets);
                        //CachedCmds.Add(new MsgYDCommand() { Title = reader.GetAttribute("title"), Targets = s });
                    }
                    reader.Read();
                }
            }
            cachedCmdsDg.ItemsSource = CachedCmds;

            // 初始化已发命令
            SendCmds = new ObservableCollection<MsgYDCommand>();
            using (XmlReader reader = XmlReader.Create("SendCmds.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd")
                    {
                        //var targets = reader.GetAttribute("targets").Split('\t');
                        //string s = string.Join(",", targets);
                        //SendCmds.Add(new MsgYDCommand() { Title = reader.GetAttribute("title"), Targets = s });
                    }
                    reader.Read();
                }
            }
            sendCmdsDg.ItemsSource = SendCmds;

            // 初始化 接收命令
            ReceivedCmds = new ObservableCollection<MsgYDCommand>();
            using (XmlReader reader = XmlReader.Create("ReceivedCmds.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd")
                    {
                        //var targets = reader.GetAttribute("targets").Split('\t');
                        //string s = string.Join(",", targets);
                        //ReceivedCmds.Add(new MsgYDCommand() { Title = reader.GetAttribute("title"), Targets = s });
                    }
                    reader.Read();
                }
            }
            receivedCmdsDg.ItemsSource = ReceivedCmds;

            BindingOperations.EnableCollectionSynchronization(SendCmds, new object());
            BindingOperations.EnableCollectionSynchronization(ReceivedCmds, new object());
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
            var cmd = SendCmds.Where(i => i.CmdSN == data.CmdSN).First();
            var station = cmd.Targets.Where(i => i.Name == data.Station).First();

            station.IsChecked = true;
            station.CheckTime = data.CheckTime;
            station.Checkee = data.Checkee;

            if (IsAllTargetChecked(cmd))
            {
                SendCmds.Remove(cmd);
                ReceivedCmds.Insert(0, cmd);
            }
        }

        private bool IsAllTargetChecked(MsgYDCommand cmd)
        {
            foreach (var item in cmd.Targets.Where(i=>i.IsSelected))
            {
                if (!item.IsChecked)
                {
                    return false;
                }
            }
            return true;
        }


        // 新建一个命令
        private void NewEdittingCmd(MsgYDCommand data)
        {
            // 如果当前窗口有命令正在编辑，则显示保存该命令的内容
            if (CmdEdittingGrid.DataContext != null)
            {
                var cmd = (MsgYDCommand)CmdEdittingGrid.DataContext;
                Inline[] tmp = new Inline[CmdParagraph.Inlines.Count];
                CmdParagraph.Inlines.CopyTo(tmp, 0);
                cmd.Content = tmp;
            }

            //新命令的上下文数据
            CmdEdittingGrid.DataContext = data;
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

            SendingCmds.Insert(0, data);
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
            if (Application.Current.Windows.OfType<CommandTemplateWindow>().Count() == 0)
            {
                CommandTemplateWindow window = new CommandTemplateWindow(eventAggregator);
                window.Show();
            }
        }

        private void ChangeEdittingCmd(object sender, MouseButtonEventArgs e)
        {
            var cmd = (MsgYDCommand)((DataGridRow)sender).DataContext;

            var cmdCurrent = (MsgYDCommand)CmdEdittingGrid.DataContext;
            Inline[] tmp = new Inline[CmdParagraph.Inlines.Count];
            CmdParagraph.Inlines.CopyTo(tmp, 0);
            cmdCurrent.Content = tmp;

            CmdEdittingGrid.DataContext = cmd;
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
            if (CmdEdittingGrid.DataContext != null)
            {
                var cmd = (MsgYDCommand)CmdEdittingGrid.DataContext;
                cmd.Content = Content2String(CmdParagraph);

                var controller = await this.ShowProgressAsync("", "发送中……");
                controller.SetIndeterminate();

                try
                {
                    await Task.Run(() =>
                                    {
                                        IO.SendMsg(cmd);
                                    });

                    await controller.CloseAsync();

                    cmd.IsEnable = false;
                    SendingCmds.Remove(cmd);
                    SendCmds.Insert(0, cmd);
                }
                catch (Exception except)
                {
                    await controller.CloseAsync();
                    MessageBox.Show(except.Message, "下达失败");
                }
            }

        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}

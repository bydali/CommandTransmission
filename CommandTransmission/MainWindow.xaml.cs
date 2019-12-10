using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        }

        private void RegisterALLEvent()
        {
            eventAggregator.GetEvent<EditNewCommand>().Unsubscribe(NewEdittingCmd);
            eventAggregator.GetEvent<EditNewCommand>().Subscribe(NewEdittingCmd);
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
    }
}

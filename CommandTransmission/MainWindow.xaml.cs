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
        private ObservableCollection<Station> allStations;

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
            // 初始化缓存命令
            CachedCmds = new ObservableCollection<MsgYDCommand>();
            using (XmlReader reader = XmlReader.Create("CachedCmds.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd")
                    {
                        CachedCmds.Add(new MsgYDCommand() { Title = reader.GetAttribute("title"), Target = reader.GetAttribute("target") });
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
                        SendCmds.Add(new MsgYDCommand() { Title = reader.GetAttribute("title"), Target = reader.GetAttribute("target") });
                    }
                    reader.Read();
                }
            }
            cachedCmdsDg.ItemsSource = CachedCmds;

            // 初始化 接收命令
            ReceivedCmds = new ObservableCollection<MsgYDCommand>();
            using (XmlReader reader = XmlReader.Create("ReceivedCmds.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd")
                    {
                        ReceivedCmds.Add(new MsgYDCommand() { Title = reader.GetAttribute("title"), Target = reader.GetAttribute("target") });
                    }
                    reader.Read();
                }
            }
            cachedCmdsDg.ItemsSource = CachedCmds;

            // 初始化受令列表
            allStations = new ObservableCollection<Station>();
            using (XmlReader reader = XmlReader.Create("MyStations.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "station")
                    {
                        allStations.Add(new Station() { Name = reader.GetAttribute("name") });
                    }
                    reader.Read();
                }
            }
            allStationDg.ItemsSource = allStations;
        }

        private void RegisterALLEvent()
        {
            eventAggregator.GetEvent<EditNewCommand>().Unsubscribe(NewEdittingCmd);
            eventAggregator.GetEvent<EditNewCommand>().Subscribe(NewEdittingCmd);
        }

        private void NewEdittingCmd(MsgYDCommand data)
        {
            CmdEdittingGrid.DataContext = data;
            CmdParagraph.Inlines.Clear();

            var lst = data.Content.Split(new string[] { "***" }, StringSplitOptions.None);
            for (int i = 0; i < lst.Length; i++)
            {
                Run r = new Run(lst[i]);
                CmdParagraph.Inlines.Add(r);

                if (i != lst.Length - 1)
                {
                    Hyperlink hl = new Hyperlink();
                    hl.Inlines.Add(new Run("____"));
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
            if (Application.Current.Windows.OfType<CommandTemplateWindow>().Count() == 0)
            {
                CommandTemplateWindow window = new CommandTemplateWindow(eventAggregator);
                window.Show();
            }
        }
    }
}

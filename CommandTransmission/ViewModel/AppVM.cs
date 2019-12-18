using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml;
using YDMSG;

namespace CommandTransmission
{
    class AppVM : BindableBase
    {
        private IEventAggregator eventAggregator;
        public ObservableCollection<MsgYDCommand> CachedCmds { get; set; }
        public ObservableCollection<MsgYDCommand> SendCmds { get; set; }
        public ObservableCollection<MsgYDCommand> ReceivedCmds { get; set; }
        public ObservableCollection<MsgYDCommand> SendingCmds { get; set; }
        private DispatcherTimer timer;

        private string appTitle;
        public string AppTitle { get => appTitle; set { SetProperty(ref appTitle, value); } }

        private string clock;
        public string Clock
        {
            get => clock;
            set
            {
                SetProperty(ref clock, value);
            }
        }

        public MsgYDCommand CurrentCmd
        {
            get => currentCmd;
            set
            {
                SetProperty(ref currentCmd, value);
            }
        }
        private MsgYDCommand currentCmd;

        internal AppVM(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            appTitle = ConfigurationManager.ConnectionStrings["ClientName"].ConnectionString; ;

            SendingCmds = new ObservableCollection<MsgYDCommand>();
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
            BindingOperations.EnableCollectionSynchronization(SendingCmds, new object());
            BindingOperations.EnableCollectionSynchronization(SendCmds, new object());

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Clock = DateTime.Now.ToString("HH:mm:ss");
        }

    
    }
}

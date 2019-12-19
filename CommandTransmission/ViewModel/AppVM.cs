using Prism.Commands;
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
        private DispatcherTimer timer;

        private string appTitle;
        public string AppTitle { get => appTitle; set { SetProperty(ref appTitle, value); } }

        private string user;

        private string clock;
        public string Clock
        {
            get => clock;
            set
            {
                SetProperty(ref clock, value);
            }
        }

        public ObservableCollection<MsgDispatchCommand> CachedCmds { get; set; }
        public ObservableCollection<MsgDispatchCommand> SendCmds { get; set; }
        public ObservableCollection<MsgDispatchCommand> ReceivedCmds { get; set; }
        public ObservableCollection<MsgDispatchCommand> SendingCmds { get; set; }
        public MsgDispatchCommand CurrentCmd
        {
            get => currentCmd;
            set
            {
                SetProperty(ref currentCmd, value);
            }
        }
        private MsgDispatchCommand currentCmd;

        public DelegateCommand CacheCmd { get; private set; }
        public DelegateCommand ApplyFor { get; private set; }
        public DelegateCommand SendCmd { get; private set; }
        public DelegateCommand AgentSign { get; private set; }

        private bool agentCanExecute;
        private bool sendCanExecute;
        private bool applyCanExecute;
        private bool cacheCanExecute;
        public bool AgentCanExecute
        {
            get => agentCanExecute;
            set => agentCanExecute = value;
        }
        public bool SendCanExecute
        {
            get => sendCanExecute;
            set => sendCanExecute = value;
        }
        public bool ApplyCanExecute
        {
            get => applyCanExecute;
            set => applyCanExecute = value;
        }
        public bool CacheCanExecute
        {
            get => cacheCanExecute;
            set => cacheCanExecute = value;
        }

        internal AppVM(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            appTitle = ConfigurationManager.ConnectionStrings["ClientName"].ConnectionString;
            user = ConfigurationManager.ConnectionStrings["User"].ConnectionString;
            AppTitle = AppTitle + "\t" + "用户：" + user;

            SendingCmds = new ObservableCollection<MsgDispatchCommand>();
            // 初始化缓存命令
            CachedCmds = new ObservableCollection<MsgDispatchCommand>();
            using (XmlReader reader = XmlReader.Create("CachedCmds.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd")
                    {
                        //var targets = reader.GetAttribute("targets").Split('\t');
                        //string s = string.Join(",", targets);
                        //CachedCmds.Add(new MsgDispatchCommand() { Title = reader.GetAttribute("title"), Targets = s });
                    }
                    reader.Read();
                }
            }
            // 初始化已发命令
            SendCmds = new ObservableCollection<MsgDispatchCommand>();
            using (XmlReader reader = XmlReader.Create("SendCmds.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd")
                    {
                        //var targets = reader.GetAttribute("targets").Split('\t');
                        //string s = string.Join(",", targets);
                        //SendCmds.Add(new MsgDispatchCommand() { Title = reader.GetAttribute("title"), Targets = s });
                    }
                    reader.Read();
                }
            }
            // 初始化 接收命令
            ReceivedCmds = new ObservableCollection<MsgDispatchCommand>();
            using (XmlReader reader = XmlReader.Create("ReceivedCmds.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd")
                    {
                        //var targets = reader.GetAttribute("targets").Split('\t');
                        //string s = string.Join(",", targets);
                        //ReceivedCmds.Add(new MsgDispatchCommand() { Title = reader.GetAttribute("title"), Targets = s });
                    }
                    reader.Read();
                }
            }
            BindingOperations.EnableCollectionSynchronization(SendingCmds, new object());
            BindingOperations.EnableCollectionSynchronization(SendCmds, new object());


            CacheCmd = new DelegateCommand(CacheExecute).ObservesProperty(() => CacheCanExecute);
            ApplyFor = new DelegateCommand(ApplyExecute).ObservesProperty(() => ApplyCanExecute);
            SendCmd = new DelegateCommand(SendExecute).ObservesProperty(() => SendCanExecute);
            AgentSign = new DelegateCommand(AgentExecute).ObservesProperty(() => AgentCanExecute);
        }

        private void AgentExecute()
        {

        }

        private void SendExecute()
        {

        }

        private void ApplyExecute()
        {

        }

        private void CacheExecute()
        {

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Clock = DateTime.Now.ToString("HH:mm:ss");
        }


    }
}

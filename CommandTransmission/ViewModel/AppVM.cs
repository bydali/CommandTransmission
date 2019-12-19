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
using System.Windows.Input;
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

        public RoutedUICommand CacheCmd { get; set; }
        public RoutedUICommand ApplyFor { get; set; }
        public RoutedUICommand SendCmd { get; set; }
        public RoutedUICommand AgentSign { get; set; }

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
            BindingOperations.EnableCollectionSynchronization(CachedCmds, new object());

            CacheCmd = new RoutedUICommand();
            ApplyFor = new RoutedUICommand();
            SendCmd = new RoutedUICommand();
            AgentSign = new RoutedUICommand();
        }

        internal void ApplyForCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CurrentCmd == null)
            {
                e.CanExecute = false;
            }
            else
            {
                if ((CurrentCmd.CmdState == CmdState.已缓存)&&
                    (CurrentCmd.NeedAuthorization==true))
                {
                    e.CanExecute = true;
                }
                else 
                {
                    e.CanExecute = false;
                }
            }
        }

        internal void SendCmdExecute(object sender, ExecutedRoutedEventArgs e)
        {

        }

        internal void SendCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        internal void AgentSignCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        internal void AgentSignExecute(object sender, ExecutedRoutedEventArgs e)
        {

        }

        internal void CacheCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CurrentCmd == null)
            {
                e.CanExecute = false;
            }
            else
            {
                if (!(CurrentCmd.CmdState==CmdState.已缓存))
                {
                    e.CanExecute = true;
                }
                else if ((CurrentCmd.CmdState == CmdState.已缓存)
                    && CurrentCmd.IsRead2Update)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Clock = DateTime.Now.ToString("HH:mm:ss");
        }


    }
}

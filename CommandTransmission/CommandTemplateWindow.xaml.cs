using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using YDMSG;

namespace CommandTransmission
{
    /// <summary>
    /// CommandTemplateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CommandTemplateWindow : Window
    {
        private IEventAggregator eventAggregator;
        private ObservableCollection<CmdTemplate> CmdTmp;

        public CommandTemplateWindow(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            GenerateTemplate();
            InitializeComponent();
        }

        /// <summary>
        /// 从配置文件加载命令模板
        /// </summary>
        private void GenerateTemplate()
        {
            ObservableCollection<Target> allTargets = new ObservableCollection<Target>();
            using (XmlReader reader = XmlReader.Create("AllTargets.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "target")
                    {
                        allTargets.Add(new Target() { Name = reader.GetAttribute("name") });
                    }
                    reader.Read();
                }
            }

            CmdTmp = new ObservableCollection<CmdTemplate>();
            using (XmlReader reader = XmlReader.Create("CmdTemplate.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd-type")
                    {
                        CmdTmp.Add(new CmdTemplate(reader.GetAttribute("type")));
                    }
                    else if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd")
                    {
                        CmdTmp.Last().CmdList.Add(new MsgDispatchCommand(
                            reader.GetAttribute("title"),
                            reader.GetAttribute("content"),
                            reader.GetAttribute("need_authorization").Equals("1"),
                            allTargets
                            ));
                    }
                    reader.Read();
                }
            }
            DataContext = CmdTmp;
        }

        /// <summary>
        /// 生成模板命令的一个副本，因而使用反序列化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((TreeView)sender).SelectedValue is MsgDispatchCommand)
            {
                var cmd = (MsgDispatchCommand)((TreeView)sender).SelectedValue;
                var cmd_copy = IO.CopySomething<MsgDispatchCommand>(cmd);
                eventAggregator.GetEvent<EditNewCommand>().
                    Publish(cmd_copy);
            }
        }
    }
}

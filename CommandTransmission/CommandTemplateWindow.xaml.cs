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

        private void GenerateTemplate()
        {
            ObservableCollection<Cmd2Station> allStations = new ObservableCollection<Cmd2Station>();
            using (XmlReader reader = XmlReader.Create("MyStations.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "station")
                    {
                        allStations.Add(new Cmd2Station() { Name = reader.GetAttribute("name") });
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
                        CmdTmp.Last().CmdList.Add(new MsgYDCommand(
                            reader.GetAttribute("title"),
                            reader.GetAttribute("content"),
                            allStations
                            ));
                    }
                    reader.Read();
                }
            }
            DataContext = CmdTmp;
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((TreeView)sender).SelectedValue is MsgYDCommand)
            {
                var cmd = (MsgYDCommand)((TreeView)sender).SelectedValue;
                string json = JsonConvert.SerializeObject(cmd, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                var cmd_copy = JsonConvert.DeserializeObject<MsgYDCommand>(json, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                eventAggregator.GetEvent<EditNewCommand>().
                    Publish(cmd_copy);
            }
        }
    }
}

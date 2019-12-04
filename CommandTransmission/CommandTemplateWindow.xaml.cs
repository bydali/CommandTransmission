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

namespace CommandTransmission
{
    /// <summary>
    /// CommandTemplateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CommandTemplateWindow : Window
    {
        private ObservableCollection<CmdTemplate> CmdTmp;

        public CommandTemplateWindow()
        {
            GenerateTemplate();
            InitializeComponent();
        }

        private void GenerateTemplate()
        {
            CmdTmp = new ObservableCollection<CmdTemplate>();
            using (XmlReader reader = XmlReader.Create("..\\..\\CmdTemplate.xml"))
            {
                while (!reader.EOF)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd-type")
                    {
                        CmdTmp.Add(new CmdTemplate(reader.GetAttribute("type")));
                    }
                    else if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "cmd")
                    {
                        CmdTmp.Last().CmdList.Add(new Cmd(reader.GetAttribute("name"), reader.GetAttribute("content")));
                    }
                    reader.Read();
                }
            }
            DataContext = CmdTmp;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandTransmission
{
    class Cmd
    {
        public string Name { get; set; }
        public string Content { get; set; }

        internal Cmd(string Name, string Content)
        {
            this.Name = Name;
            this.Content = Content;
        }
    }

    class CmdTemplate
    {
        public string Type { get; set; }
        public ObservableCollection<Cmd> CmdList { get; set; }

        internal CmdTemplate(string Type)
        {
            this.Type = Type;
            CmdList = new ObservableCollection<Cmd>();
        }
    }
}

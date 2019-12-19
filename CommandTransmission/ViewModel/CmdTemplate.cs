using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YDMSG;

namespace CommandTransmission
{
    class CmdTemplate
    {
        public string Type { get; set; }
        public ObservableCollection<MsgDispatchCommand> CmdList { get; set; }

        internal CmdTemplate(string Type)
        {
            this.Type = Type;
            CmdList = new ObservableCollection<MsgDispatchCommand>();
        }
    }
}

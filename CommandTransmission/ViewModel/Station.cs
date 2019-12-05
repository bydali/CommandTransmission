using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandTransmission
{
    class Station
    {
        public string Name { get; set; }
        // 前一条命令是否签收
        public bool IsCheck { get; set; }
        public string Signee { get; set; }
        // 签收时间
        public DateTime Time { get; set; }

    }
}

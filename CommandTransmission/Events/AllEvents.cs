using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YDMSG;

namespace CommandTransmission
{
    public class EditNewCommand : PubSubEvent<MsgDispatchCommand> { }
    public class ReceiptCommand : PubSubEvent<MsgReceipt> { }
}


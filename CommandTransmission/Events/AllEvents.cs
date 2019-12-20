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
    public class SignCommand : PubSubEvent<MsgSign> { }
    public class AgentSignCommand : PubSubEvent<MsgSign> { }
    public class CacheCommand : PubSubEvent<MsgDispatchCommand> { }
    public class ApproveCommand : PubSubEvent<MsgDispatchCommand> { }
    public class TransmitCommand : PubSubEvent<MsgDispatchCommand> { }
}


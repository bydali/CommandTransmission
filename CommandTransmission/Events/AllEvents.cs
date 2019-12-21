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
    public class SignCommand : PubSubEvent<MsgCommandSign> { }
    public class AgentSignCommand : PubSubEvent<MsgCommandSign> { }
    public class CacheCommand : PubSubEvent<MsgDispatchCommand> { }
    public class ApproveCommand : PubSubEvent<MsgDispatchCommand> { }
    public class TransmitCommand : PubSubEvent<MsgDispatchCommand> { }

    public class NotifyMain : PubSubEvent<MsgSpeedCommand> { }
    public class CheckSpeedCommand : PubSubEvent<MsgSpeedCommand> { }
    public class PassSpeedCommand : PubSubEvent<MsgSpeedCommand> { }
    public class CacheSpeedCommand : PubSubEvent<MsgSpeedCommand> { }
    public class ActiveSpeedCommand : PubSubEvent<MsgSpeedCommand> { }
    public class ExecuteSpeedCommand : PubSubEvent<MsgSpeedCommand> { }
}


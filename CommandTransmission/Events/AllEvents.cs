using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YDMSG;

namespace CommandTransmission
{
    #region 本地事件

    /// <summary>
    /// 命令模板传递命令到主界面
    /// </summary>
    public class EditNewCommand : PubSubEvent<MsgDispatchCommand> { }

    /// <summary>
    /// 新建列控窗口传递命令到主界面
    /// </summary>
    public class NotifyMain : PubSubEvent<MsgSpeedCommand> { }

    #endregion

    #region 网络流入事件

    public class CacheCommand : PubSubEvent<MsgDispatchCommand> { }
    public class ApproveCommand : PubSubEvent<MsgDispatchCommand> { }
    public class TransmitCommand : PubSubEvent<MsgDispatchCommand> { }
    public class AgentSignCommand : PubSubEvent<MsgCommandSign> { }
    public class SignCommand : PubSubEvent<MsgCommandSign> { }
    public class CheckSpeedCommand : PubSubEvent<MsgSpeedCommand> { }
    public class CacheSpeedCommand : PubSubEvent<MsgSpeedCommand> { }
    public class ActiveSpeedCommand : PubSubEvent<MsgSpeedCommand> { }
    public class PassSpeedCommand : PubSubEvent<MsgSpeedCommand> { }
    public class ExecuteSpeedCommand : PubSubEvent<MsgSpeedCommand> { }

    #endregion
}


using DSIM.Communications;
using Newtonsoft.Json;
using Prism.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YDMSG;

namespace CommandTransmission
{
    class IO
    {
        private static IEventAggregator eventAggregator;
        private static MQHelper _mqHelper;

        public static async void ReceiveMsg(IEventAggregator aggregator)
        {
            eventAggregator = aggregator;

            //MQHelper.ConnectionString = ConfigurationManager.ConnectionStrings["RabbitMQ"].ConnectionString;
            //_mqHelper = new MQHelper();
            //_mqHelper.ClientSubscriptionId = ConfigurationManager.ConnectionStrings["ClientName"].ConnectionString + "-" +
            //    ConfigurationManager.ConnectionStrings["User"].ConnectionString;
            //_mqHelper.MessageArrived += RabbitMQ_MessageArrived;
            //_mqHelper.Topics.Add("DSIM.Command.Update");
            //_mqHelper.Topics.Add("DSIM.Command.Approve");
            //_mqHelper.Topics.Add("DSIM.Command.Transmit");
            //_mqHelper.Topics.Add("DSIM.Command.Sign");
            //_mqHelper.Topics.Add("DSIM.Command.AgentSign");
            //_mqHelper.Topics.Add("DSIM.Command.Check");
            //_mqHelper.Topics.Add("DSIM.Command.SpeedCache");
            //_mqHelper.Topics.Add("DSIM.Command.Active");
            //_mqHelper.Topics.Add("DSIM.Command.Execute");
            //_mqHelper.Subcribe();

            // 以下为测试代码
            try
            {
                ConnectionFactory factory = new ConnectionFactory { HostName = "39.108.177.237", Port = 5672, UserName = "admin", Password = "admin" };
                using (IConnection conn = factory.CreateConnection())
                {
                    using (IModel im = conn.CreateModel())
                    {
                        List<string> allTopic = new List<string>()
                        {
                        "DSIM.Command.Update",
                        "DSIM.Command.Approve",
                        "DSIM.Command.Transmit",
                        "DSIM.Command.Sign",
                        "DSIM.Command.AgentSign",
                        "DSIM.Command.Check",
                        "DSIM.Command.SpeedCache",
                        "DSIM.Command.Active",
                        "DSIM.Command.Execute",};

                        var queue = ConfigurationManager.ConnectionStrings["User"].ConnectionString;

                        im.ExchangeDeclare("amq.topic", ExchangeType.Topic, durable: true);
                        im.QueueDeclare(queue);
                        foreach (var item in allTopic)
                        {
                            im.QueueBind(queue, "amq.topic", item);
                        }

                        await Task.Run(() =>
                        {
                            while (true)
                            {
                                BasicGetResult res = im.BasicGet(queue, true);
                                if (res != null)
                                {
                                    var json = Encoding.UTF8.GetString(res.Body);

                                    var split = json.LastIndexOf("/");
                                    var suffix = json.Substring(split + 1);
                                    var content = json.Substring(0, split);

                                    switch (suffix)
                                    {
                                        case ("DSIM.Command.Update"):
                                            var data1 = JsonConvert.DeserializeObject<YDMSG.MsgDispatchCommand>(content);
                                            eventAggregator.GetEvent<CacheCommand>().Publish(data1);
                                            break;
                                        case ("DSIM.Command.Approve"):
                                            var data2 = JsonConvert.DeserializeObject<YDMSG.MsgDispatchCommand>(content);
                                            eventAggregator.GetEvent<ApproveCommand>().Publish(data2);
                                            break;
                                        case ("DSIM.Command.Transmit"):
                                            var data3 = JsonConvert.DeserializeObject<YDMSG.MsgDispatchCommand>(content);
                                            eventAggregator.GetEvent<TransmitCommand>().Publish(data3);
                                            break;
                                        case ("DSIM.Command.Sign"):
                                            var data4 = JsonConvert.DeserializeObject<YDMSG.MsgCommandSign>(content);
                                            eventAggregator.GetEvent<SignCommand>().Publish(data4);
                                            break;
                                        case ("DSIM.Command.AgentSign"):
                                            var data5 = JsonConvert.DeserializeObject<YDMSG.MsgCommandSign>(content);
                                            eventAggregator.GetEvent<AgentSignCommand>().Publish(data5);
                                            break;
                                        case ("DSIM.Command.Check"):
                                            var data6 = JsonConvert.DeserializeObject<YDMSG.MsgSpeedCommand>(content);
                                            eventAggregator.GetEvent<CheckSpeedCommand>().Publish(data6);
                                            break;
                                        case ("DSIM.Command.SpeedCache"):
                                            var data7 = JsonConvert.DeserializeObject<YDMSG.MsgSpeedCommand>(content);
                                            eventAggregator.GetEvent<CacheSpeedCommand>().Publish(data7);
                                            break;
                                        case ("DSIM.Command.Active"):
                                            var data8 = JsonConvert.DeserializeObject<YDMSG.MsgSpeedCommand>(content);
                                            eventAggregator.GetEvent<ActiveSpeedCommand>().Publish(data8);
                                            break;
                                        case ("DSIM.Command.Execute"):
                                            var data9 = JsonConvert.DeserializeObject<YDMSG.MsgSpeedCommand>(content);
                                            eventAggregator.GetEvent<ExecuteSpeedCommand>().Publish(data9);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message);
            }
        }

        //private static void RabbitMQ_MessageArrived(object sender, MsgCategoryEnum e)
        //{
        //    switch (e)
        //    {
        //        case (MsgCategoryEnum.CommandUpdate):
        //            eventAggregator.GetEvent<CacheCommand>().Publish((MsgDispatchCommand)sender);
        //            break;
        //        case (MsgCategoryEnum.CommandApprove):
        //            eventAggregator.GetEvent<ApproveCommand>().Publish((MsgDispatchCommand)sender);
        //            break;
        //        case (MsgCategoryEnum.CommandTransmit):
        //            eventAggregator.GetEvent<TransmitCommand>().Publish((MsgDispatchCommand)sender);
        //            break;
        //        case (MsgCategoryEnum.CommandSign):
        //            eventAggregator.GetEvent<SignCommand>().Publish((MsgCommandSign)sender);
        //            break;
        //        case (MsgCategoryEnum.CommandAgentSign):
        //            eventAggregator.GetEvent<AgentSignCommand>().Publish((MsgCommandSign)sender);
        //            break;
        //        case (MsgCategoryEnum.CommandCheck):
        //            eventAggregator.GetEvent<CheckSpeedCommand>().Publish((MsgSpeedCommand)sender);
        //            break;
        //        case (MsgCategoryEnum.SpeedCache):
        //            eventAggregator.GetEvent<CacheSpeedCommand>().Publish((MsgSpeedCommand)sender);
        //            break;
        //        case (MsgCategoryEnum.CommandActive):
        //            eventAggregator.GetEvent<ActiveSpeedCommand>().Publish((MsgSpeedCommand)sender);
        //            break;
        //        case (MsgCategoryEnum.CommandExecute):
        //            eventAggregator.GetEvent<ExecuteSpeedCommand>().Publish((MsgSpeedCommand)sender);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        /// <summary>
        /// 将消息发往指定主题
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="topic"></param>
        public static void SendMsg(object msg, string topic)
        {
            //switch (msg.GetType().Name)
            //{
            //    case ("MsgDispatchCommand"):
            //        ((MsgDispatchCommand)msg).Topic = topic;
            //        _mqHelper.Publish((MsgDispatchCommand)msg);
            //        break;
            //    case ("MsgCommandSign"):
            //        ((MsgCommandSign)msg).Topic = topic;
            //        _mqHelper.Publish((MsgCommandSign)msg);
            //        break;
            //    case ("MsgSpeedCommand"):
            //        ((MsgSpeedCommand)msg).Topic = topic;
            //        _mqHelper.Publish((MsgSpeedCommand)msg);
            //        break;
            //    default:
            //        break;
            //}

            // 以下为测试代码
            ConnectionFactory factory = new ConnectionFactory { HostName = "39.108.177.237", Port = 5672, UserName = "admin", Password = "admin" };
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel im = conn.CreateModel())
                {
                    string json = JsonConvert.SerializeObject(msg);
                    json += "/" + topic;
                    byte[] message = Encoding.UTF8.GetBytes(json);

                    im.ExchangeDeclare("amq.topic", ExchangeType.Topic, durable: true);
                    im.BasicPublish("amq.topic", topic, null, message);


                }
            }
        }

        /// <summary>
        /// 通过序列化拷贝一个新的内存占用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T CopySomething<T>(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            var copy = JsonConvert.DeserializeObject<T>(json);

            return copy;
        }

    }
}

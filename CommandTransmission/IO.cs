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

            // 以下为测试代码
            try
            {
                ConnectionFactory factory = new ConnectionFactory { HostName = "39.108.177.237", Port = 5672, UserName = "admin", Password = "admin" };
                using (IConnection conn = factory.CreateConnection())
                {
                    using (IModel im = conn.CreateModel())
                    {
                        List<string> allTopic = new List<string>()
                        { "DSIM.Command.Create",
                        "DSIM.Command.Update",
                        "DSIM.Command.Approve",
                        "DSIM.Command.Transmit",
                        "DSIM.Command.Sign",
                        "DSIM.Command.AgentSign"};

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
                                        case ("DSIM.Command.Create"):
                                            var data = JsonConvert.DeserializeObject<MsgDispatchCommand>(content);
                                            eventAggregator.GetEvent<CacheCommand>().Publish(data);
                                            break;
                                        case ("DSIM.Command.Update"):
                                            var data1 = JsonConvert.DeserializeObject<MsgDispatchCommand>(content);
                                            eventAggregator.GetEvent<CacheCommand>().Publish(data1);
                                            break;
                                        case ("DSIM.Command.Approve"):
                                            var data2 = JsonConvert.DeserializeObject<MsgDispatchCommand>(content);
                                            eventAggregator.GetEvent<ApproveCommand>().Publish(data2);
                                            break;
                                        case ("DSIM.Command.Transmit"):
                                            var data3 = JsonConvert.DeserializeObject<MsgDispatchCommand>(content);
                                            eventAggregator.GetEvent<TransmitCommand>().Publish(data3);
                                            break;
                                        case ("DSIM.Command.Sign"):
                                            var data4 = JsonConvert.DeserializeObject<MsgSign>(content);
                                            eventAggregator.GetEvent<SignCommand>().Publish(data4);
                                            break;
                                        case ("DSIM.Command.AgentSign"):
                                            var data5 = JsonConvert.DeserializeObject<MsgSign>(content);
                                            eventAggregator.GetEvent<AgentSignCommand>().Publish(data5);
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

        public static void SendMsg(object msg,string topic)
        {
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

        private static void RabbitMQ_MessageArrived(object sender, MsgCategoryEnum e)
        {
            eventAggregator.GetEvent<PubSubEvent<string>>().Publish("asdf");
        }

        public static T CopySomething<T>(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            var copy = JsonConvert.DeserializeObject<T>(json);

            return copy;
        }

    }
}

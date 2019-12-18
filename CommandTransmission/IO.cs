﻿using DSIM.Communications;
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
            //_mqHelper = new MQHelper
            //{
            //    ClientSubscriptionId = ConfigurationManager.ConnectionStrings["ClientID"].ConnectionString
            //};
            //_mqHelper.MessageArrived += RabbitMQ_MessageArrived;

            // 以下为测试代码
            try
            {

                ConnectionFactory factory = new ConnectionFactory { HostName = "39.108.177.237", Port = 5672, UserName = "admin", Password = "admin" };
                using (IConnection conn = factory.CreateConnection())
                {
                    using (IModel im = conn.CreateModel())
                    {
                        List<string> allTopic = new List<string>() { "回执信息" };

                        im.ExchangeDeclare("amq.topic", ExchangeType.Topic, durable: true);
                        im.QueueDeclare("center");
                        foreach (var item in allTopic)
                        {
                            im.QueueBind("center", "amq.topic", item);
                        }

                        await Task.Run(() =>
                        {
                            while (true)
                            {
                                BasicGetResult res = im.BasicGet("center", true);
                                if (res != null)
                                {
                                    var json = Encoding.UTF8.GetString(res.Body);

                                    var data = JsonConvert.DeserializeObject<MsgReceipt>(json);
                                    eventAggregator.GetEvent<ReceiptCommand>().Publish(data);
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

        public static void SendMsg(object msg)
        {
            // 以下为测试代码
            ConnectionFactory factory = new ConnectionFactory { HostName = "39.108.177.237", Port = 5672, UserName = "admin", Password = "admin" };
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel im = conn.CreateModel())
                {
                    im.ExchangeDeclare("amq.topic", ExchangeType.Topic, durable: true);

                    string json = JsonConvert.SerializeObject(msg);

                    byte[] message = Encoding.UTF8.GetBytes(json);
                    im.BasicPublish("amq.topic", "调度命令", null, message);
                }
            }
        }

        private static void RabbitMQ_MessageArrived(object sender, MsgCategoryEnum e)
        {
            eventAggregator.GetEvent<PubSubEvent<string>>().Publish("asdf");
        }

    }
}

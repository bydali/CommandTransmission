using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandTransmission.IO
{
    public static class WriteToPort
    {
        public static void SendMsg()
        {
            ConnectionFactory factory = new ConnectionFactory { HostName = "39.108.177.237", UserName = "admin", Password = "admin", VirtualHost = "/" };
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel im = conn.CreateModel())
                {
                    im.ExchangeDeclare("rabbitmq_route", ExchangeType.Direct);
                    im.QueueDeclare("rabbitmq_query", false, false, false, null);
                    im.QueueBind("rabbitmq_query", "rabbitmq_route", ExchangeType.Direct, null);
                    for (int i = 0; i < 1000; i++)
                    {
                        byte[] message = Encoding.UTF8.GetBytes("Hello Lv");
                        im.BasicPublish("rabbitmq_route", ExchangeType.Direct, null, message);
                        Console.WriteLine("send:" + i);
                    }
                }
            }

        }

    }
}

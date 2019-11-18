using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandTransmission.IO
{
    class WriteToPort
    {
        public static void SendMsg()
        {
            ConnectionFactory factory = new ConnectionFactory { HostName = "39.108.177.237", UserName = "admin", Password = "admin" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("amq.topic", "topic", true, false, null);

                    var routingKey = "q1";
                    var message = "Hello World!";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "amq.topic",
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine("Sent {0}", message);
                }
            }

        }

    }
}

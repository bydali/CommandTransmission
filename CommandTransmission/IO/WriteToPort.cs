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
            //int i = 0;
            //while (true)
            //{
            //    var factory = new ConnectionFactory() { HostName = "39.108.177.237", Port = 5672, UserName = "admin", Password = "admin", VirtualHost = "/" };
            //    using (var connection = factory.CreateConnection())
            //    using (var channel = connection.CreateModel())
            //    {
            //        channel.ExchangeDeclare(exchange: "",
            //                                type: "topic");
            //        var body = Encoding.UTF8.GetBytes(i.ToString());
            //        channel.BasicPublish(exchange: "topic_logs",
            //                             routingKey: "anonymous.info",
            //                             basicProperties: null,
            //                             body: body);
            //        Console.WriteLine("asfd");
            //    }
            //}

        }

    }
}

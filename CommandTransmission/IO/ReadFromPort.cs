using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandTransmission.IO
{
    class ReadFromPort
    {
        public static void ReceiveMsg()
        {
            ConnectionFactory factory = new ConnectionFactory { HostName = "39.108.177.237", UserName = "admin", Password = "admin" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("Q2", true, consumer);

                    while (true)
                    {
                        var ea = consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        Console.WriteLine("Received {0}", message);
                    }
                }
            }
        }
    }
}

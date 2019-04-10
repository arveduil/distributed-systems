using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Send
{

  public  class Receive
    {
        public static void ReceiveMessage()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var queueName = "hello";
            var routingKey = "key";
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var queue = channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                String EXCHANGE_NAME = "exchange1";
                channel.QueueBind(queueName, EXCHANGE_NAME, routingKey);
                channel.ExchangeDeclare(EXCHANGE_NAME,"FANOUT");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}

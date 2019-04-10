using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace Receive
{

    public class Receive
    {
        //private readonly IConnection connection;
        //private readonly IModel channel;
        //private readonly string replyQueueName;
        //private readonly EventingBasicConsumer consumer;
        //private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        //private readonly IBasicProperties props;
        string queueName = "hello";
        string routingKey = "key";
        string type = "direct";
        string EXCHANGE_NAME = "exchange1";

        public Receive()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
               channel.ExchangeDeclare(EXCHANGE_NAME, type);

                channel.QueueDeclare(queue: queueName, durable: false,
                  exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: "rpc_queue",
                  autoAck: false, consumer: consumer);
                Console.WriteLine(" [x] Awaiting RPC requests");

                consumer.Received += (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        int n = int.Parse(message);
                        Console.WriteLine(" [.] fib({0})", message);
                        response = "myResp";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" [.] " + e.Message);
                        response = "";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        channel.BasicPublish(exchange: EXCHANGE_NAME, routingKey: props.ReplyTo,
                          basicProperties: replyProps, body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag,
                          multiple: false);
                    }
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }



        //channel.ExchangeDeclare(exchange_name_doctor_to_technician, type);

        //var queueName = channel.QueueDeclare().QueueName;

        //foreach (var skill in skills)
        //{
        //    channel.QueueBind(queueName, exchange_name_doctor_to_technician, routingKey);
        //}

        //var consumer = new EventingBasicConsumer(channel);
        //consumer.Received += (model, ea) =>
        //{
        //    var body = ea.Body;
        //    var key = ea.RoutingKey;
        //    var message = Encoding.UTF8.GetString(body);
        //    Console.WriteLine(" [x] Received {0}, key {1}", message, key);
        //};
        //channel.BasicConsume(queue: queueName,
        //                     autoAck: true,
        //                     consumer: consumer);

        //Console.WriteLine(" Press [enter] to exit.");
        //Console.ReadLine();



    } 
}

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Send
{
    public class Technician
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;
        private int ID;
        private static string exchangeName1 = "lol111";

        private static string queueName = "rpc_queue1";


        public Technician(int id, string[] skills)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            ID = id;
            using (var connection = factory.CreateConnection())
            {

                Console.WriteLine(string.Format(" Technician {0} is read to take our orders.", ID));

                foreach (var skill in skills)
                {
                    using (var channel = connection.CreateModel())
                    {

                        channel.ExchangeDeclare(exchange: exchangeName1, type: "direct");
                        channel.QueueDeclare(queue: skill, durable: false,
                          exclusive: false, autoDelete: false, arguments: null);
                        channel.BasicQos(0, 1, false);
                        Console.WriteLine(string.Format("   I can fix: {0}", skill));

                        channel.QueueBind(queue: skill,
                                    exchange: exchangeName1,
                                    routingKey: skill);

                        var consumer = new EventingBasicConsumer(channel);
                        channel.BasicConsume(queue: skill,
                          autoAck: false, consumer: consumer);

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
                                Console.WriteLine("Technician {0} get message {1} on routingKey {2}", ID, message, ea.RoutingKey);
                                Console.WriteLine("Technician {0} is processing {1}", ID, ea.RoutingKey);

                                response = message + " done";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(" ERROR " + e.Message);
                                response = "";
                            }
                            finally
                            {
                                Console.WriteLine("Technician {0} send response {1}", ID, response);

                                var responseBytes = Encoding.UTF8.GetBytes(response);
                                channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                                  basicProperties: replyProps, body: responseBytes);
                                channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                  multiple: false);
                            }
                        };
                    }
                }
            }
        }
    }
}

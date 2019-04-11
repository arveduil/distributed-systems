using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Technician
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
        private static string exchangeName1 = "abc";
        private static string exchangeName2 = "abc";

        public Technician( string[] skills)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
           var connection = factory.CreateConnection();

                Console.WriteLine(" Technician is ready to take our orders.");
                var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: exchangeName1, type: "direct");

            channel.BasicQos(0, 1, true);
            foreach (var skill in skills)
            {
                channel.QueueDeclare(queue: skill, durable: false,
                     exclusive: false, autoDelete: false, arguments: null);
                Console.WriteLine(string.Format("   I can fix: {0}", skill));
                    channel.QueueBind(queue: skill,
                                exchange: exchangeName1,
                                routingKey: skill);
            }
            var consumer = new EventingBasicConsumer(channel);


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
                                Console.WriteLine("Technician  get message {1} on routingKey {2}",message, ea.RoutingKey);
                                Console.WriteLine("Technician  is processing {1}",  ea.RoutingKey);

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


            foreach (var skill in skills)
            {
                channel.BasicConsume(queue: skill,
                        autoAck: false, consumer: consumer);
            }
        }
    }
}

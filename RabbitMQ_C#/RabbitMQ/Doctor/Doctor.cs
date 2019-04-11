using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace Doctor
{


    public class Doctor
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;
        private static string exchangeName1 = "abc";

        public Doctor()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;



            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;



            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    respQueue.Add(response);
                }
            };
        }

        public string Call(Request request)
        {
            string patientName = request.Name;
            string type = request.Type;

            var message = type + " " +patientName;
            Console.WriteLine("{Doctor sends message: {0}",  message);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            channel.ExchangeDeclare(exchange: exchangeName1,
                                    type: "direct");
            channel.QueueBind(type, exchangeName1, type);
            channel.BasicPublish(
                exchange: exchangeName1,
                routingKey: type,
                basicProperties: props,
                body: messageBytes);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            var response =  respQueue.Take();
            return response;
        }

        public void Close()
        {
            connection.Close();
        }
    }
}

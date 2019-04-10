using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace Send
{
    public class Send
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;
        string queueName = "hello";
        string routingKey = "key";
        string type = "direct";
        string EXCHANGE_NAME = "exchange1";


        public  Send(string[] args)
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

        //    var factory = new ConnectionFactory() { HostName = "localhost" };
        //    using (var connection = factory.CreateConnection())
        //    {
        //        channel = connection.CreateModel();

        //         channel.ExchangeDeclare(exchange: EXCHANGE_NAME,
        //                         type: type);

        //        replyQueueName = channel.QueueDeclare().QueueName;
        //        consumer = new EventingBasicConsumer(channel);

        //        props = channel.CreateBasicProperties();
        //        var correlationId = Guid.NewGuid().ToString();
        //        props.CorrelationId = correlationId;
        //        props.ReplyTo = replyQueueName;


        //        consumer.Received += (model, ea) =>
        //        {
        //            var body = ea.Body;
        //            var response = Encoding.UTF8.GetString(body);
        //            if (ea.BasicProperties.CorrelationId == correlationId)
        //            {
        //                respQueue.Add(response);
        //            }
        //        };

        //        channel.QueueDeclare(queue: "rpc_queue", durable: false,
        //            exclusive: false, autoDelete: false, arguments: null);
        //            channel.BasicQos(0, 1, false);
        //            var consumer = new EventingBasicConsumer(channel);
        //            channel.BasicConsume(queue: "rpc_queue",
        //              autoAck: false, consumer: consumer);
        //            Console.WriteLine(" [x] Awaiting RPC requests");

        //            props = channel.CreateBasicProperties();
        //            var correlationId = Guid.NewGuid().ToString();
        //            props.CorrelationId = correlationId;
        //            props.ReplyTo = replyQueueName;

        //            consumer.Received += (model, ea) =>
        //            {
        //                var body = ea.Body;
        //                var response = Encoding.UTF8.GetString(body);
        //                if (ea.BasicProperties.CorrelationId == correlationId)
        //                {
        //                    respQueue.Add(response);
        //                }
        //            };

        //            string message = "Hello World!";
        //            body = Encoding.UTF8.GetBytes(message);

        //           // channel.QueueBind(queueName, EXCHANGE_NAME, routingKey);
        //            channel.BasicPublish(exchange: EXCHANGE_NAME,
        //                                 routingKey: routingKey,
        //                                 basicProperties: null,
        //                                 body: body);
        //            Console.WriteLine(" [x] Sent {0}", message);
        //        }

        //        Console.WriteLine(" Press [enter] to exit.");
        //        Console.ReadLine();

        //}

        public string Call(string message,string key)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: EXCHANGE_NAME,
                routingKey: key,
                basicProperties: props,
                body: messageBytes);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return respQueue.Take(); ;
        }


    }
}
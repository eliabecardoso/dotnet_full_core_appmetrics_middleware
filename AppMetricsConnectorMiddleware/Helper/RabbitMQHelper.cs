using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AppOwinAppMetrics.Middlewares.Metrics.Helper
{
    public class RabbitMQHelper : IRabbitMQHelper
    {
        public ConnectionFactory GetConnectionFactory()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost", //dockerbackoffice
                UserName = "guest",
                Password = "guest",
            };

            return connectionFactory;
        }

        public IConnection CreateConnection(ConnectionFactory connectionFactory)
        {
            return connectionFactory.CreateConnection();
        }

        public QueueDeclareOk CreateQueue(string queueName, IModel channel)
        {
            QueueDeclareOk queue;

            queue = channel.QueueDeclare(queueName, false, false, false, null);

            return queue;
        }

        public void WriteMessageOnQueue(string message, string queueName, IModel channel)
        {
            channel.BasicPublish(exchange: string.Empty, routingKey: queueName, basicProperties: null, body: Encoding.UTF8.GetBytes(message));
        }

        private uint RetrieveMessageCount(string queueName, IModel channel) //change public if use
        {
            uint count = channel.MessageCount(queueName);

            return count;
        }
    }
}
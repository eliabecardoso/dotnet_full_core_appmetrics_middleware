using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppOwinAppMetrics.Middlewares.Metrics.Helper
{
    public interface IRabbitMQHelper
    {
        ConnectionFactory GetConnectionFactory();
        IConnection CreateConnection(ConnectionFactory connectionFactory);
        QueueDeclareOk CreateQueue(string queueName, IModel connection);
        void WriteMessageOnQueue(string message, string queueName, IModel connection);
    }
}
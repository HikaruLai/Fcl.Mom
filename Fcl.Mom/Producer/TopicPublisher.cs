using System;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Fcl.Mom.Producer
{
    public class TopicPublisher : IMessageSender
    {
        #region Properties
        private ILogger logger { get; set; }
        private IConnection connection { get; set; }
        private IModel channel { get; set; }
        private string brokerURI { get; set; }
        private string exchangeName { get; set; }
        private string queueName { get; set; }
        public string routingKey { get; set; }
        #endregion

        public TopicPublisher(ILoggerFactory loggerFactory, string brokerURI, string exchangeName, string queueName)
        {
            #region Initialize Properties
            this.logger = loggerFactory.CreateLogger<TopicPublisher>();
            this.brokerURI = brokerURI;
            this.exchangeName = exchangeName;
            this.queueName = queueName;
            this.disposed = true;
            #endregion
        }

        public void SendMessage(string message)
        {
            var sendBytes = Encoding.UTF8.GetBytes(message);
            var properties = this.channel.CreateBasicProperties();
            properties.Persistent = true;
            this.channel.BasicPublish(this.exchangeName, this.routingKey, properties, sendBytes);
        }

        public void SetRoutingKey(string routingKey)
        {
            this.routingKey = routingKey;
        }

        public void Start()
        {
            if (string.IsNullOrWhiteSpace(this.brokerURI))
            {
                throw new Exception("Parameter Error...");
            }
            if (this.disposed)
            {
                var connectionFactory = new ConnectionFactory { Uri = new Uri(this.brokerURI) };
                this.connection = connectionFactory.CreateConnection();
                this.channel = connection.CreateModel();
                this.channel.ExchangeDeclare(exchange: this.exchangeName, type: ExchangeType.Topic, durable: true);
                this.channel.QueueDeclare(queue: this.queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                this.channel.QueueBind(queue: this.queueName, exchange: this.exchangeName, routingKey: this.routingKey);
                this.channel.BasicQos(0, 1, false);
                this.disposed = false;
            }
        }

        public void Stop()
        {
            this.Dispose();
        }

        #region Dispose method

        private bool disposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if (disposing)
                {
                    logger.LogDebug("Run Disposing...");
                    this.channel.Close();
                    this.channel.Dispose();
                    this.connection.Close();
                    this.connection.Dispose();
                }
                this.disposed = true;
            }
        }

        #endregion

        ~TopicPublisher()
        {
            // Calling Dispose(false) is optimal in terms of readability and maintainability.
            this.Dispose(false);
        }
    }
}

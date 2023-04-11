using System;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Fcl.Mom.Consumer
{
    public class TopicSubscriber : IMessageReceiver
    {
        #region Properties
        private ILogger logger { get; set; }
        private IConnection connection { get; set; }
        private IModel channel { get; set; }
        private string brokerURI { get; set; }
        private string exchangeName { get; set; }
        private string queueName { get; set; }
        private string routingKey { get; set; }
        #endregion

        public TopicSubscriber(ILoggerFactory loggerFactory, string brokerURI, string exchangeName, string queueName)
        {
            #region Initialize Properties
            this.logger = loggerFactory.CreateLogger<TopicSubscriber>();
            this.brokerURI = brokerURI;
            this.exchangeName = exchangeName;
            this.queueName = queueName;
            this.disposed = true;
            #endregion
        }

        public event EventHandler<BasicDeliverEventArgs> OnMessageReceived;

        public void PositiveAck(ulong deliveryTag)
        {
            //return ack to remove this message
            this.channel.BasicAck(deliveryTag, multiple: false);
        }

        public void NegativeAck(ulong deliveryTag)
        {
            //return reject, requeue this message
            this.channel.BasicReject(deliveryTag, requeue: true);
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
                this.channel.ExchangeDeclare(this.exchangeName, ExchangeType.Topic, durable: true);
                this.channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                this.channel.QueueBind(this.queueName, this.exchangeName, this.routingKey);
                this.channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                logger.LogDebug("Waiting for messages...");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (s, ea) => OnMessageReceived?.Invoke(this, ea);
                this.channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
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

        ~TopicSubscriber()
        {
            // Calling Dispose(false) is optimal in terms of readability and maintainability.
            this.Dispose(false);
        }
    }
}

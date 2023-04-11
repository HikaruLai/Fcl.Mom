using System;
using RabbitMQ.Client.Events;

namespace Fcl.Mom.Consumer
{
    public interface IMessageReceiver : IDisposable
    {
        /// <summary>
        /// EventHandler when receive message from MQ Server
        /// </summary>
        event EventHandler<BasicDeliverEventArgs> OnMessageReceived;

        /// <summary>
        /// Used for positive acknowledgements
        /// </summary>
        void PositiveAck(ulong deliveryTag);

        /// <summary>
        /// Used for negative acknowledgements
        /// </summary>
        void NegativeAck(ulong deliveryTag);

        /// <summary>
        /// Set routingKey
        /// </summary>
        /// <param name="routingKey"></param>
        void SetRoutingKey(string routingKey);

        /// <summary>
        /// Connect to MQ Server
        /// </summary>
        void Start();

        /// <summary>
        /// Close Connection
        /// </summary>
        void Stop();
    }
}

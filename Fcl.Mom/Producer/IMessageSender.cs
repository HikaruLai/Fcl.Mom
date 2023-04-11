using System;

namespace Fcl.Mom.Producer
{
    public interface IMessageSender : IDisposable
    {
        /// <summary>
        /// Send message to MQ Server
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(string message);

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

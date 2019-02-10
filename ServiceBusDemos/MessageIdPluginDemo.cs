using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Plugins;
using Newtonsoft.Json;
using System.Threading.Tasks;
using ServiceBusDemos.Models;

namespace ServiceBusDemos
{
    static class MessageIdPluginDemo
    {
        public static async Task SendMessagesAsync(IQueueClient queueClient, int numOfMessages)
        {
            // Initialize and register the message ID plugin
            var messageIdPlugin = new MessageIdPlugin((msg) => ByteArrayToHash(msg.Body));
            queueClient.RegisterPlugin(messageIdPlugin);

            for (var i = 0; i < numOfMessages; i++)
            {
                // Initialize the message
                var location = new Coordinates {Latitude = 34.0522, Longitude = -118.2437};
                var messageBody = JsonConvert.SerializeObject(location);
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                // Send the message. The Message ID property does not have to be
                // set because it is being managed by the registered plugin.
                await queueClient.SendAsync(message);

                Console.WriteLine($"Coordinates sent: {location.Latitude}, {location.Longitude}");
            }
        }

        private static string ByteArrayToHash(byte[] msg)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                return Convert.ToBase64String(sha1.ComputeHash(msg));
            }
        }

    }
}

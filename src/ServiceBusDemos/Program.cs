using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Configuration;

namespace ServiceBusDemos
{
    internal class Program
    {
        static string ServiceBusConnectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];
        static string QueueName = ConfigurationManager.AppSettings["QueueName"];

        static void Main(string[] args)
        {            
            TestMessageIdPlugin().GetAwaiter().GetResult();            
        }

        static async Task TestMessageIdPlugin()
        {            
            Console.WriteLine("Press any key to send messages.");
            Console.ReadKey();

            // Instantiate the queue client
            IQueueClient queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            // Invoke the message ID plugin demo that will send
            // duplicate messages to the queue.
            await MessageIdPluginDemo.SendMessagesAsync(queueClient, 5);

            // Cleanup
            await queueClient.CloseAsync();
        }

    }
}

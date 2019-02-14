using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using FleetUpdates.Models;
using FleetUpdates.Utils;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FleetUpdates
{
    public static class TruckEventProcessor
    {
        private static HttpClient Client = null;
        private static string DomainAccessKey = Environment.GetEnvironmentVariable("DomainAccessKey");
        private static string DomainEndpoint = Environment.GetEnvironmentVariable("DomainEndpoint");
        private static string SubscriptionId = Environment.GetEnvironmentVariable("SubscriptionId");
        private static string DomainName = Environment.GetEnvironmentVariable("DomainName");
        private static string ResourceGroupName = Environment.GetEnvironmentVariable("ResourceGroupName");

        [FunctionName("TruckEventProcessor")]
        public static async Task Run(
            [EventHubTrigger("%EventHubName%", Connection = "EventHubConnectionString")] EventData[] events, 
            ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (var eventData in events)
            {
                try
                {
                    log.LogInformation("TruckEventProcessor triggered.");

                    // Retrieve the truck update
                    var messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    var truckUpdate = JsonConvert.DeserializeObject<TruckUpdate>(messageBody);

                    // Obtain an instance of an Http client
                    if (Client == null) Client = InitializeHttpClient();                    

                    // Set the event type and subject
                    var eventType = "Fleet.TruckUpdate";
                    var subject = "testSubject";

                    // Determine the topic based off of the score                    
                    string topicName = "";
                    if (truckUpdate.Score > 90)
                        topicName = "companya";
                    else if (truckUpdate.Score > 70)
                        topicName = "companyb";
                    else if (truckUpdate.Score > 50)
                        topicName = "companyc";

                    var source = CloudEventHelper.FormatSourceForDomainEndpoint(SubscriptionId, 
                        ResourceGroupName, DomainName, topicName, subject);

                    await CloudEventHelper.SendEvent(Client, truckUpdate, eventType, source);
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.
            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }

        private static HttpClient InitializeHttpClient()
        {
            HttpClient client = new HttpClient { BaseAddress = new Uri(DomainEndpoint) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("aeg-sas-key", DomainAccessKey);
            return client;
        }

    }
}

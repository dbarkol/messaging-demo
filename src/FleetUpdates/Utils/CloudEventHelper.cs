using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FleetUpdates.Models;
using Newtonsoft.Json;

namespace FleetUpdates.Utils
{
    public static class CloudEventHelper
    {
        public static async Task SendEvent(HttpClient client, 
            TruckUpdate truckUpdate, 
            string eventType,
            string source)
        {
            var cloudEvent = new CloudEvent<TruckUpdate>
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = eventType ,
                EventTypeVersion = "1.0",
                CloudEventVersion = "0.1",
                Data = truckUpdate,
                Source = source,
                EventTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            };

            var json = JsonConvert.SerializeObject(cloudEvent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(string.Empty, content);
        }

        public static string FormatSourceForTopicEndpoint(string subscriptionId, string resourceGroupName, string topicName, string subject)
        {
            // Format the cloud event source property for a topic endpoint. 
            // Sample: /subscriptions/<subscription-id>/resourceGroups/<resource-group-name>/providers/Microsoft.EventGrid/topics/<topic-name>

            return
                $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.EventGrid/topics/{topicName}#{subject}";
        }

        public static string FormatSourceForDomainEndpoint(string subscriptionId, string resourceGroupName, string domainName, string topicName, string subject)
        {
            // Format the cloud event source property for a domain endpoint.
            // The topic name is optional.

            // Sample:
            // /subscriptions/<subscription-id>/resourceGroups/<resource-group-name>/providers/Microsoft.EventGrid/domains/<domain-name>/topics/<topic-name>#subject

            return
                $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.EventGrid/domains/{domainName}/topics/{topicName}#{subject}";
        }
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using FleetUpdates.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FleetUpdates.Utils;

namespace FleetUpdates
{
    public static class TruckEventHttp
    {
        private static Producer<long, string> KafkaProducer = null;
        private static string KafkaTopic = Environment.GetEnvironmentVariable("EventHubName");

        /// <summary>
        /// This function is invoked by an HTTP request. It's purpose is to pass along
        /// the contents to an Event Hub. 
        /// </summary>
        [FunctionName("TruckEventHttp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("TruckEventHttp invoked");

            // Retrieve the message body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var update = data.update.ToString();

            // Calculate the score and instantiate 
            // an instance of a truck update message.
            var score = Score.GetScore(update);
            var truckUpdate = new TruckUpdate {
                Id = Guid.NewGuid(),
                Score = score,
                Update = update
            };

            // Retrieve an instance of the Kafka producer
            if (KafkaProducer == null) KafkaProducer = KafkaHelper.GetKafkaProducer(context, log);

            // Send to Kafka endpoint
            var msg = JsonConvert.SerializeObject(truckUpdate);
            var deliveryReport = await KafkaProducer.ProduceAsync(KafkaTopic, 
                                            DateTime.UtcNow.Ticks, 
                                            msg);

            return (ActionResult)new OkObjectResult($"Your score: {score}");
        }
    }
}

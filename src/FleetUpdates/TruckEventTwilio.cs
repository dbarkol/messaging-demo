using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FleetUpdates.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Twilio.TwiML;
using FleetUpdates.Models;

namespace FleetUpdates
{
    public static class TruckEventTwilio
    {
        private static Producer<long, string> KafkaProducer = null;
        private static string KafkaTopic = Environment.GetEnvironmentVariable("EventHubName");

        /// <summary>
        /// This function is invoked by an HTTP request from Twilio. It's
        /// purpose is to pass along the contents to an Event Hub. 
        /// </summary>
        [FunctionName("TruckEventTwilio")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("TruckEventTwilio invoked");

            // Read the request content, return if invalid.
            var requestBody = new StreamReader(req.Body).ReadToEnd();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Not a valid request");
            }

            // Extract the message body from the payload and
            // calculate the score.
            var update = GetMessageBody(requestBody);
            var score = Score.GetScore(update);
            var truckUpdate = new TruckUpdate
            {
                Id = Guid.NewGuid(),
                Score = score,
                Update = update
            };

            // Get an instance of the Kafka producer
            if (KafkaProducer == null) KafkaProducer = KafkaHelper.GetKafkaProducer(context, log);

            // Send to Kafka endpoint
            var msg = JsonConvert.SerializeObject(truckUpdate);
            var deliveryReport = await KafkaProducer.ProduceAsync(KafkaTopic,
                DateTime.UtcNow.Ticks,
                msg);

            // Format the response 
            var response = new MessagingResponse().Message($"Thank you. Your score: {score}");
            var twiml = response.ToString();
            twiml = twiml.Replace("utf-16", "utf-8");

            // Use content result to ensure that the payload in the 
            // response is in XML.
            return new ContentResult { Content = twiml, ContentType = "application/xml" };
        }

        private static string GetMessageBody(string data)
        {
            var formValues = data.Split('&')
                .Select(value => value.Split('='))
                .ToDictionary(pair => Uri.UnescapeDataString(pair[0]).Replace("+", " "),
                    pair => Uri.UnescapeDataString(pair[1]).Replace("+", " "));

            return formValues["Body"];
        }
    }
}

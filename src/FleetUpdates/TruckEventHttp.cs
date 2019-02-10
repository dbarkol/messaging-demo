using System;
using System.IO;
using System.Threading.Tasks;
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
        /// <summary>
        /// This function is invoked by an HTTP request. It's purpose is to pass along
        /// the contents to an Event Hub. 
        /// </summary>
        [FunctionName("TruckEventHttp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("TruckEventHttp invoked");

            // Retrieve the message body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var update = data.update.ToString();

            // Calculate the score
            var score = Score.GetScore(update);

            // TODO: Send to Kafka endpoint

            return (ActionResult)new OkObjectResult($"Your score: {score}");
        }
    }
}

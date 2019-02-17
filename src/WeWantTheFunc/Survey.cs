using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Newtonsoft.Json;
using Twilio.TwiML;
using WeWantTheFunc.Models;

namespace WeWantTheFunc
{
    public static class Survey
    {
        [FunctionName("Survey")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Survey function triggered");

            // Read the request content into a string, return
            // a bad request error code if it's empty.
            var requestBody = new StreamReader(req.Body).ReadToEnd();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Not a valid request");
            }

            // Extract the message body from the payload and
            // create a feedback instance.
            var body = GetMessageBody(requestBody);
            var f = new Feedback
            {
                Id = Guid.NewGuid(),
                Message = body.Trim(),
                Score = GetScore(body)
            };

            // Publish to event grid
            await SendFeedback(f);

            // Format the response 
            var response = new MessagingResponse().Message("Thank you!"); 
            var twiml = response.ToString();
            twiml = twiml.Replace("utf-16", "utf-8");

            // Use content result to ensure that the payload in the 
            // response is in XML.
            return new ContentResult { Content = twiml, ContentType = "application/xml" };
        }

        private static async Task SendFeedback(Feedback f)
        {
            var topicHostName = System.Environment.GetEnvironmentVariable("TopicHostName");
            var topicKey = System.Environment.GetEnvironmentVariable("TopicKey");

            ServiceClientCredentials credentials = new TopicCredentials(topicKey);

            var client = new EventGridClient(credentials);

            var events = new List<EventGridEvent>
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    Data = f,
                    EventTime = DateTime.Now,
                    EventType = f.Score > 70 ? "Positive" : "Negative",
                    Subject = "eventgrid/demo/feedback",
                    DataVersion = "1.0"
                }
            };

            await client.PublishEventsAsync(
                topicHostName,
                events);
        }

        private static int GetScore(string message)
        {
            ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials())
            {
                Endpoint = "https://westus2.api.cognitive.microsoft.com"
            }; 

            var results = client.SentimentAsync(
                new MultiLanguageBatchInput(
                    new List<MultiLanguageInput>()
                    {
                        new MultiLanguageInput("en", "0", message)
                    })).Result;

            if (results.Documents.Count == 0) return 0;
            var score = results.Documents[0].Score.GetValueOrDefault();
            var fixedScore = (int)(score * 100);

            return fixedScore;
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

    internal class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string _textAnalyticsApi = System.Environment.GetEnvironmentVariable("TextAnalyticsApiKey");

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", _textAnalyticsApi);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}

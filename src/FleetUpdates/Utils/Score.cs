
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;

namespace FleetUpdates.Utils
{
    public static class Score
    {
        public static int GetScore(string message)
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
    }
}

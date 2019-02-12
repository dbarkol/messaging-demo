using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;

namespace FleetUpdates.Utils
{
    public static class KafkaHelper
    {

        public static Producer<long, string> GetKafkaProducer(ExecutionContext context, ILogger log)
        {
            var brokerList = Environment.GetEnvironmentVariable("EventHubFqdn");
            var connStr = Environment.GetEnvironmentVariable("EventHubConnectionString");
            var caCertFileName = Environment.GetEnvironmentVariable("CaCertFileName");
            var cacertlocation = Path.Combine(context.FunctionAppDirectory, caCertFileName);

            // Local testing
            if (Environment.GetEnvironmentVariable("IsLocal") == "1")
            {
                cacertlocation = ".\\cacert.pem";
            }

            var config = new Dictionary<string, object>
            {
                {"bootstrap.servers", brokerList},
                {"security.protocol", "SASL_SSL"},
                {"sasl.mechanism", "PLAIN"},
                {"sasl.username", "$ConnectionString"},
                {"sasl.password", connStr},
                {"ssl.ca.location", cacertlocation}
            };

            return new Producer<long, string>(config,
                new LongSerializer(),
                new StringSerializer(Encoding.UTF8));
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FleetUpdates.Models
{
    public class TruckUpdate
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("update")]
        public string Update { get; set; }

        [JsonProperty("Score")]
        public int Score { get; set; }
    }
}

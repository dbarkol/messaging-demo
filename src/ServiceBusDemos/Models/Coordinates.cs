using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ServiceBusDemos.Models
{
    public class Coordinates
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
}

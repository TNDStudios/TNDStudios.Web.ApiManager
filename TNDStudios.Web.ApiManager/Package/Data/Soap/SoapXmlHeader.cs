﻿using Newtonsoft.Json;
using System;

namespace TNDStudios.Web.ApiManager.Data.Soap
{
    /// <summary>
    /// Object to hold the XML header tag in the Soap message
    /// </summary>
    [Serializable]
    [JsonObject]
    public class SoapXmlHeader : SoapBase
    {
        /// <summary>
        /// The XML version of the Soap message
        /// </summary>
        [JsonProperty(PropertyName = "@version")]
        public String Version { get; set; }

        /// <summary>
        /// How the message is encoded
        /// </summary>
        [JsonProperty(PropertyName = "@encoding")]
        public String Encoding { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SoapXmlHeader() : base()
        {
        }
    }
}

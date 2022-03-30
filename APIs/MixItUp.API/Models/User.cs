﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class User
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string TwitchID { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int? ViewingMinutes { get; set; }
        
        [DataMember]
        public bool IsSpecialtyExcluded { get; set; }

        [DataMember]
        public List<CurrencyAmount> CurrencyAmounts { get; set; } = new List<CurrencyAmount>();

        [DataMember]
        public List<InventoryAmount> InventoryAmounts { get; set; } = new List<InventoryAmount>();
    }
}

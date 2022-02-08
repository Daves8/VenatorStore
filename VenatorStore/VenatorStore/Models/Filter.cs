using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VenatorStore.Models
{
    public class Filter
    {
        public bool House { get; set; }
        public bool Shop { get; set; }
        public bool Horse { get; set; }
        public bool Jacket { get; set; }
        public bool Pants { get; set; }
        public bool Boots { get; set; }
        public bool Sword { get; set; }
        public bool Bow { get; set; }
        public bool Knife { get; set; }
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }

        public Filter()
        {
            MinPrice = 1.0;
            MaxPrice = 1000000.0;

            House = true;
            Shop = true;
            Horse = true;
            Jacket = true;
            Pants = true;
            Boots = true;
            Sword = true;
            Bow = true;
            Knife = true;
        }
    }
}
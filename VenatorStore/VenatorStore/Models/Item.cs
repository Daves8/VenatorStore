using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VenatorStore.Models
{
    public class Item
    {
        public int Id { get; set; }
        public Category Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; }
    }

    public enum Category
    {
        House,
        Shop,
        Horse,
        Jacket,
        Pants,
        Boots,
        Sword,
        Bow,
        Knife
    }
}
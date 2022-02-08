using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VenatorStore.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfRegistration { get; set; }
        public string Roles { get; set; }

        public double Gold { get; set; }
        public string Items { get; set; }
        public string ItemsInCart { get; set; }

        public User()
        {
            Roles = JsonConvert.SerializeObject(new List<string>());
            Items = JsonConvert.SerializeObject(new List<int>());
            ItemsInCart = JsonConvert.SerializeObject(new List<int>());
        }
    }
}
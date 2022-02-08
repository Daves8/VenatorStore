using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VenatorStore.Models
{
    public class StoreContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }

        public StoreContext(DbContextOptions<StoreContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User[]
                {
                new User {
                    Id=1,
                    Username="admin",
                    Password="admin",
                    FullName="",
                    Email="",
                    PhoneNumber="",
                    DateOfRegistration=DateTime.Now,
                    Roles=JsonConvert.SerializeObject(new List<string>(){ "admin","moderator","user"}),
                    Gold=1000000,
                    Items=JsonConvert.SerializeObject(new List<int>()),
                    ItemsInCart=JsonConvert.SerializeObject(new List<int>())
                }
                });
        }
    }
}
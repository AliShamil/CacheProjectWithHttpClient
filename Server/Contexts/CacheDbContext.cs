using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModelsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Contexts;

public class CacheDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

        var connectionString = configuration.GetConnectionString("ConStr");

        optionsBuilder.UseSqlServer(connectionString);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KeyValue>().HasKey(e => e.Key);
        base.OnModelCreating(modelBuilder);
    }

   public DbSet<KeyValue>? KeyValues { get; set; }
}

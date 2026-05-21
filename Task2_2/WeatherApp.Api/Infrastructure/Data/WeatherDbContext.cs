using Microsoft.EntityFrameworkCore;
using WeatherApp.Api.Domain.Models;

namespace WeatherApp.Api.Infrastructure.Data;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
    {
    }

    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<City> Cities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Data
        modelBuilder.Entity<Country>().HasData(
            new Country { Id = 1, Code = "ID", Name = "Indonesia" },
            new Country { Id = 2, Code = "US", Name = "United States" },
            new Country { Id = 3, Code = "JP", Name = "Japan" }
        );

        modelBuilder.Entity<City>().HasData(
            new City { Id = 1, Name = "Jakarta", CountryCode = "ID" },
            new City { Id = 2, Name = "Surabaya", CountryCode = "ID" },
            new City { Id = 3, Name = "Bali", CountryCode = "ID" },
            new City { Id = 4, Name = "New York", CountryCode = "US" },
            new City { Id = 5, Name = "Los Angeles", CountryCode = "US" },
            new City { Id = 6, Name = "Tokyo", CountryCode = "JP" },
            new City { Id = 7, Name = "Osaka", CountryCode = "JP" }
        );
    }
}

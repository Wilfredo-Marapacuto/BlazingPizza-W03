using Microsoft.EntityFrameworkCore;

namespace BlazingPizza;

public class PizzaStoreContext : DbContext
{
    public PizzaStoreContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<PizzaSpecial> Specials { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<Pizza> Pizzas { get; set; }

    public DbSet<Topping> Toppings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PizzaTopping>()
            .HasKey(p => new { p.PizzaId, p.ToppingId });

        modelBuilder.Entity<PizzaTopping>()
            .HasOne<Pizza>()
            .WithMany(p => p.Toppings)
            .HasForeignKey(p => p.PizzaId);

        modelBuilder.Entity<PizzaTopping>()
            .HasOne(p => p.Topping)
            .WithMany()
            .HasForeignKey(p => p.ToppingId);
    }
}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web_API_new.Models;

namespace Web_API_new.Data;

public class DataContext : IdentityDbContext<UserModel>
{
    public DbSet<UserModel> Users { get; set; }
    public DbSet<TeamModel> Teams { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TeamModel>()
            .HasOne(t => t.Leader)
            .WithOne()
            .HasForeignKey<TeamModel>(t => t.LeaderId);
        
    }
}
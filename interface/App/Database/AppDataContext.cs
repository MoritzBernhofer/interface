using App.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace App.Database;

public class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : DbContext(options)
{
    public DbSet<Dummy> Dummies => Set<Dummy>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}
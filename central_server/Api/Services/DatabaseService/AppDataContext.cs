using central_server.Services.DatabaseService.models;
using Microsoft.EntityFrameworkCore;

namespace central_server.Services.DatabaseService;

public class AppDataContext(DbContextOptions<AppDataContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
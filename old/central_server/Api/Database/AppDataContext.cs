using central_server.Database.models;
using Microsoft.EntityFrameworkCore;

namespace central_server.Database;

public class AppDataContext(DbContextOptions<AppDataContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserLog> UserLogs { get; set; }

    public DbSet<IotService> IotServices { get; set; }
    public DbSet<IotDeviceLog> IotDeviceLogs { get; set; }
    public DbSet<IotDevice> IotDevices { get; set; }

    public DbSet<ClientServer> ClientServers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
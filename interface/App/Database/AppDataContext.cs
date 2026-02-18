using App.Database.Model;
using App.Database.Model.Iot;
using App.Database.Model.UserRelated;
using Microsoft.EntityFrameworkCore;

namespace App.Database;

public class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : DbContext(options)
{
    public DbSet<IotDevice> IotDevices => Set<IotDevice>();
    public DbSet<IotDeviceLog> IotDeviceLogs => Set<IotDeviceLog>();
    public DbSet<IotService> IotServices => Set<IotService>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserLog> UserLogs => Set<UserLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
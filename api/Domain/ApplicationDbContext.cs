using Microsoft.EntityFrameworkCore;
using Passly.Entities;

namespace Passly.Domain;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {}

    public DbSet<User> Users { get; set; }
    public DbSet<Otp> Otps { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<VaultItem> VaultItems { get; set; }
    
}
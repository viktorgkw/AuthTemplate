using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public class ProfilesDbContext(DbContextOptions<ProfilesDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public override DbSet<ApplicationUser> Users { get; set; }

    public override DbSet<ApplicationRole> Roles { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users", "identity");

            entity.HasKey(x => x.Id);

            entity
                .Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasColumnType("VARCHAR(25)");

            entity
                .Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasColumnType("VARCHAR(25)");

            entity
                .Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasColumnType("BOOLEAN");

            entity
                .HasOne(u => u.Address)
                .WithOne()
                .HasForeignKey<Address>();
        });

        builder.Entity<ApplicationRole>()
            .ToTable("roles", "identity");

        builder
            .Entity<IdentityUserClaim<string>>()
            .ToTable("user_claims", "identity");

        builder
            .Entity<IdentityRoleClaim<string>>()
            .ToTable("role_claims", "identity");

        builder.Entity<Address>(entity =>
        {
            entity.ToTable("addresses", "identity");

            entity.HasKey(x => x.Id);

            entity
                .Property(x => x.Country)
                .HasColumnName("country")
                .HasColumnType("VARCHAR(25)");

            entity.Property(x => x.City)
                .HasColumnName("city")
                .HasColumnType("VARCHAR(25)");

            entity.Property(x => x.Street)
                .HasColumnName("street")
                .HasColumnType("VARCHAR(35)");

            entity.Property(x => x.ZipCode)
                .HasColumnName("zip_code")
                .HasColumnType("VARCHAR(5)");
        });
    }
}

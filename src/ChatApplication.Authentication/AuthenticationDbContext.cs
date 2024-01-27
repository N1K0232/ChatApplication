using ChatApplication.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Authentication;

public class AuthenticationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>,
        ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public AuthenticationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(builder =>
        {
            builder.Property(user => user.FirstName).HasMaxLength(256).IsRequired();
            builder.Property(user => user.LastName).HasMaxLength(256).IsRequired(false);

            builder.Property(user => user.ProfileImagePath).HasMaxLength(512).IsRequired(false);
            builder.Property(user => user.RefreshToken).HasMaxLength(512).IsRequired(false);
        });

        modelBuilder.Entity<ApplicationUserRole>(builder =>
        {
            builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });

            builder.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .IsRequired();

            builder.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .IsRequired();
        });
    }
}
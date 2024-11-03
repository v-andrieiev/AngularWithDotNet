using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) : IdentityDbContext<AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>,
    IdentityUserToken<int>>(options)
{
    public DbSet<UserLike> Likes { get; set;}
    public DbSet<Message> Messages { get; set;}
    public DbSet<Group> Groups { get; set;}
    public DbSet<Connection> Connections { get; set;}
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.Entity<AppRole>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        builder.Entity<UserLike>()
            .HasKey(k => new {k.SourceUserId, k.TargetUserId});

        builder.Entity<UserLike>()
            .HasOne(s => s.SourceUser) // Вказуємо, що UserLike має одне посилання на SourceUser.
            .WithMany(l => l.LikedUsers) // Вказуємо, що SourceUser може мати багато LikedUsers.
            .HasForeignKey(s => s.SourceUserId) // Налаштовуємо зовнішній ключ для SourceUserId.
            .OnDelete(DeleteBehavior.Cascade); // Видаляємо UserLike-ів, коли видаляється SourceUser.

        builder.Entity<UserLike>()
            .HasOne(s => s.TargetUser) // Вказуємо, що UserLike має одне посилання на TargetUser.
            .WithMany(l => l.LikedByUsers) // Вказуємо, що TargetUser може мати багато LikedByUsers.
            .HasForeignKey(s => s.TargetUserId) // Налаштовуємо зовнішній ключ для TargetUserId.
            .OnDelete(DeleteBehavior.Cascade); // Видаляємо UserLike-ів, коли видаляється TargetUser.

        builder.Entity<Message>()
            .HasOne(x => x.Recipient)
            .WithMany(x => x.MessageReceived)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
            .HasOne(x => x.Sender)
            .WithMany(x => x.MessageSent)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

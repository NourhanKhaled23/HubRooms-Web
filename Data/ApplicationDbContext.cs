using AdvancedChat.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvancedChat.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();

    public DbSet<ChatRoomMember> ChatRoomMembers => Set<ChatRoomMember>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ChatRoom>(entity =>
        {
            entity.HasIndex(room => room.Name).IsUnique();
            entity.Property(room => room.Name).HasMaxLength(80);
            entity.Property(room => room.Description).HasMaxLength(240);
        });

        builder.Entity<ChatRoomMember>(entity =>
        {
            entity.HasIndex(member => new { member.ChatRoomId, member.UserId }).IsUnique();
            entity.HasOne(member => member.ChatRoom)
                .WithMany(room => room.Members)
                .HasForeignKey(member => member.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(member => member.User)
                .WithMany()
                .HasForeignKey(member => member.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ChatMessage>(entity =>
        {
            entity.Property(message => message.Text).HasMaxLength(2000);
            entity.HasOne(message => message.ChatRoom)
                .WithMany(room => room.Messages)
                .HasForeignKey(message => message.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(message => message.User)
                .WithMany()
                .HasForeignKey(message => message.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

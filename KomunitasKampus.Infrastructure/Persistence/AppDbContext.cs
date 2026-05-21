using System.Linq.Expressions;
using KomunitasKampus.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Post> Posts => Set<Post>();

    public DbSet<PostMedia> PostMedia => Set<PostMedia>();

    public DbSet<Membership> Memberships => Set<Membership>();

    public DbSet<Like> Likes => Set<Like>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Share> Shares => Set<Share>();

    public DbSet<Story> Stories => Set<Story>();

    public DbSet<StoryView> StoryViews => Set<StoryView>();

    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();

    public DbSet<ChatParticipant> ChatParticipants => Set<ChatParticipant>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<ChatReadStatus> ChatReadStatuses => Set<ChatReadStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureAccount(modelBuilder);
        ConfigureOrganization(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigurePost(modelBuilder);
        ConfigurePostMedia(modelBuilder);
        ConfigureMembership(modelBuilder);
        ConfigureLike(modelBuilder);
        ConfigureComment(modelBuilder);
        ConfigureShare(modelBuilder);
        ConfigureStory(modelBuilder);
        ConfigureStoryView(modelBuilder);
        ConfigureChatRoom(modelBuilder);
        ConfigureChatParticipant(modelBuilder);
        ConfigureMessage(modelBuilder);
        ConfigureChatReadStatus(modelBuilder);

        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();

        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditTimestamps();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default
    )
    {
        ApplyAuditTimestamps();

        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        ApplyAuditTimestamps();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
                entry.Entity.DeletedAt = null;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = utcNow;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Property(entity => entity.CreatedAt).IsModified = false;

                entry.Entity.DeletedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");

            var deletedAtProperty = Expression.Property(
                parameter,
                nameof(BaseEntity.DeletedAt)
            );

            var nullValue = Expression.Constant(null, typeof(DateTime?));

            var filter = Expression.Equal(deletedAtProperty, nullValue);

            var lambda = Expression.Lambda(filter, parameter);

            modelBuilder
                .Entity(entityType.ClrType)
                .HasQueryFilter(lambda);
        }
    }

    private static void ConfigureAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(account => account.Id);

            entity.Property(account => account.Username)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(account => account.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(account => account.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(account => account.Role)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(account => account.AvatarUrl)
                .HasMaxLength(500);

            entity.HasIndex(account => account.Username)
                .IsUnique();

            entity.HasIndex(account => account.Email)
                .IsUnique();

            entity.HasOne(account => account.Organization)
                .WithOne(organization => organization.Account)
                .HasForeignKey<Organization>(organization => organization.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(account => account.User)
                .WithOne(user => user.Account)
                .HasForeignKey<User>(user => user.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(organization => organization.Id);

            entity.Property(organization => organization.AccountId)
                .IsRequired();

            entity.Property(organization => organization.OrganizationName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(organization => organization.Description)
                .HasColumnType("text");

            entity.Property(organization => organization.Slug)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(organization => organization.University)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(organization => organization.AccountId)
                .IsUnique();

            entity.HasIndex(organization => organization.OrganizationName)
                .IsUnique();

            entity.HasIndex(organization => organization.Slug)
                .IsUnique();
        });
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.Property(user => user.AccountId)
                .IsRequired();

            entity.Property(user => user.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(user => user.University)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(user => user.AccountId)
                .IsUnique();
        });
    }

    private static void ConfigurePost(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(post => post.Id);

            entity.Property(post => post.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(post => post.Caption)
                .HasColumnType("text");

            entity.Property(post => post.IsPinned)
                .HasDefaultValue(false);

            entity.Property(post => post.PinOrder);

            entity.Property(post => post.Visibility)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(post => post.LikeCount)
                .HasDefaultValue(0);

            entity.Property(post => post.CommentCount)
                .HasDefaultValue(0);

            entity.Property(post => post.ShareCount)
                .HasDefaultValue(0);

            entity.HasOne(post => post.Organization)
                .WithMany(organization => organization.Posts)
                .HasForeignKey(post => post.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasCheckConstraint(
                "ck_posts_pin_order_max_3",
                "pin_order IS NULL OR pin_order BETWEEN 1 AND 3"
            );
        });
    }

    private static void ConfigurePostMedia(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostMedia>(entity =>
        {
            entity.HasKey(media => media.Id);

            entity.Property(media => media.MediaType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(media => media.FileUrl)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(media => media.FileSizeBytes)
                .IsRequired();

            entity.Property(media => media.OrderIndex)
                .IsRequired();

            entity.Property(media => media.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.HasOne(media => media.Post)
                .WithMany(post => post.Media)
                .HasForeignKey(media => media.PostId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureMembership(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(membership => membership.Id);

            entity.Property(membership => membership.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(membership => membership.InviteType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(membership => membership.RequestedAt)
                .IsRequired();

            entity.Property(membership => membership.ResolvedAt);

            entity.HasOne(membership => membership.Account)
                .WithMany(account => account.Memberships)
                .HasForeignKey(membership => membership.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(membership => membership.Organization)
                .WithMany(organization => organization.Memberships)
                .HasForeignKey(membership => membership.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(membership => new
            {
                membership.AccountId,
                membership.OrganizationId
            }).IsUnique();
        });
    }

    private static void ConfigureLike(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(like => like.Id);

            entity.HasOne(like => like.Post)
                .WithMany(post => post.Likes)
                .HasForeignKey(like => like.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(like => like.User)
                .WithMany(user => user.Likes)
                .HasForeignKey(like => like.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(like => new
            {
                like.PostId,
                like.UserId
            }).IsUnique();
        });
    }

    private static void ConfigureComment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(comment => comment.Id);

            entity.Property(comment => comment.Content)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(comment => comment.DeletedReason)
                .HasMaxLength(255);

            entity.HasOne(comment => comment.Post)
                .WithMany(post => post.Comments)
                .HasForeignKey(comment => comment.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(comment => comment.User)
                .WithMany(user => user.Comments)
                .HasForeignKey(comment => comment.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(comment => comment.DeletedBy)
                .WithMany(account => account.DeletedComments)
                .HasForeignKey(comment => comment.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureShare(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Share>(entity =>
        {
            entity.HasKey(share => share.Id);

            entity.HasOne(share => share.Post)
                .WithMany(post => post.Shares)
                .HasForeignKey(share => share.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(share => share.User)
                .WithMany(user => user.Shares)
                .HasForeignKey(share => share.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(share => new
            {
                share.PostId,
                share.UserId
            }).IsUnique();
        });
    }

    private static void ConfigureStory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Story>(entity =>
        {
            entity.HasKey(story => story.Id);

            entity.Property(story => story.MediaType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(story => story.MediaUrl)
                .HasMaxLength(500);

            entity.Property(story => story.TextContent)
                .HasMaxLength(280);

            entity.Property(story => story.IsExpired)
                .HasDefaultValue(false);

            entity.Property(story => story.ExpiresAt)
                .IsRequired();

            entity.HasOne(story => story.Organization)
                .WithMany(organization => organization.Stories)
                .HasForeignKey(story => story.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureStoryView(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoryView>(entity =>
        {
            entity.HasKey(storyView => storyView.Id);

            entity.HasOne(storyView => storyView.Story)
                .WithMany(story => story.StoryViews)
                .HasForeignKey(storyView => storyView.StoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(storyView => storyView.Account)
                .WithMany(account => account.StoryViews)
                .HasForeignKey(storyView => storyView.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(storyView => new
            {
                storyView.StoryId,
                storyView.AccountId
            }).IsUnique();
        });
    }

    private static void ConfigureChatRoom(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasKey(room => room.Id);

            entity.Property(room => room.Name)
                .HasMaxLength(150);

            entity.Property(room => room.IsMainGroup)
                .HasDefaultValue(false);

            entity.Property(room => room.RoomType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(room => room.IsInviteOnly)
                .HasDefaultValue(false);

            entity.HasOne(room => room.Organization)
                .WithMany(organization => organization.ChatRooms)
                .HasForeignKey(room => room.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureChatParticipant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatParticipant>(entity =>
        {
            entity.HasKey(participant => participant.Id);

            entity.Property(participant => participant.JoinedAt)
                .IsRequired();

            entity.HasOne(participant => participant.Room)
                .WithMany(room => room.Participants)
                .HasForeignKey(participant => participant.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(participant => participant.Account)
                .WithMany(account => account.ChatParticipants)
                .HasForeignKey(participant => participant.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(participant => new
            {
                participant.RoomId,
                participant.AccountId
            }).IsUnique();
        });
    }

    private static void ConfigureMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(message => message.Id);

            entity.Property(message => message.Content)
                .HasColumnType("text")
                .IsRequired();

            entity.Property(message => message.SentAt)
                .IsRequired();

            entity.HasOne(message => message.Room)
                .WithMany(room => room.Messages)
                .HasForeignKey(message => message.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(message => message.Sender)
                .WithMany(account => account.SentMessages)
                .HasForeignKey(message => message.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(message => message.DeletedBy)
                .WithMany(account => account.DeletedMessages)
                .HasForeignKey(message => message.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureChatReadStatus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatReadStatus>(entity =>
        {
            entity.HasKey(readStatus => readStatus.Id);

            entity.Property(readStatus => readStatus.LastReadAt)
                .IsRequired();

            entity.HasOne(readStatus => readStatus.Room)
                .WithMany(room => room.ReadStatuses)
                .HasForeignKey(readStatus => readStatus.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(readStatus => readStatus.Account)
                .WithMany(account => account.ChatReadStatuses)
                .HasForeignKey(readStatus => readStatus.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(readStatus => new
            {
                readStatus.RoomId,
                readStatus.AccountId
            }).IsUnique();
        });
    }
}
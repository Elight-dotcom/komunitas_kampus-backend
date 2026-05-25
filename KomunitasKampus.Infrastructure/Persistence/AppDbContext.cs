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

    public DbSet<Notification> Notifications => Set<Notification>();

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
        ConfigureNotification(modelBuilder);
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

            entity.HasIndex(post => new
            {
                post.OrganizationId,
                post.IsPinned,
                post.PinOrder,
                post.CreatedAt
            });
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

            entity.HasIndex(media => new
            {
                media.PostId,
                media.OrderIndex
            });
        });
    }

    private static void ConfigureMembership(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.ToTable("memberships");

            entity.HasKey(membership => membership.Id);

            entity.Property(membership => membership.Id)
                .HasColumnName("id");

            entity.Property(membership => membership.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            entity.Property(membership => membership.OrganizationId)
                .HasColumnName("organization_id")
                .IsRequired();

            entity.Property(membership => membership.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(membership => membership.InviteType)
                .HasColumnName("invite_type")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(membership => membership.RequestedAt)
                .HasColumnName("requested_at")
                .IsRequired();

            entity.Property(membership => membership.ResolvedAt)
                .HasColumnName("resolved_at");

            entity.Property(membership => membership.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(membership => membership.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(membership => membership.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasOne(membership => membership.Account)
                .WithMany(account => account.Memberships)
                .HasForeignKey(membership => membership.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(membership => membership.Organization)
                .WithMany(organization => organization.Memberships)
                .HasForeignKey(membership => membership.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(membership => new
            {
                membership.AccountId,
                membership.OrganizationId
            })
                .IsUnique()
                .HasDatabaseName("ux_memberships_account_org_pending_accepted")
                .HasFilter("\"deleted_at\" IS NULL AND \"status\" IN ('Pending', 'Accepted')");

            entity.HasIndex(membership => new
            {
                membership.OrganizationId,
                membership.Status,
                membership.InviteType
            })
                .HasDatabaseName("ix_memberships_org_status_invite_type");

            entity.HasQueryFilter(membership => membership.DeletedAt == null);
        });
    }

    private static void ConfigureNotification(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasKey(notification => notification.Id);

            entity.Property(notification => notification.Id)
                .HasColumnName("id");

            entity.Property(notification => notification.RecipientId)
                .HasColumnName("recipient_id")
                .IsRequired();

            entity.Property(notification => notification.ActorId)
                .HasColumnName("actor_id");

            entity.Property(notification => notification.Type)
                .HasColumnName("type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(notification => notification.ReferenceId)
                .HasColumnName("reference_id");

            entity.Property(notification => notification.IsRead)
                .HasColumnName("is_read")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(notification => notification.ReadAt)
                .HasColumnName("read_at");

            entity.Property(notification => notification.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(notification => notification.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(notification => notification.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasOne(notification => notification.Recipient)
                .WithMany()
                .HasForeignKey(notification => notification.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(notification => notification.Actor)
                .WithMany()
                .HasForeignKey(notification => notification.ActorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(notification => new
            {
                notification.RecipientId,
                notification.IsRead,
                notification.CreatedAt
            })
                .HasDatabaseName("ix_notifications_recipient_read_created");

            entity.HasQueryFilter(notification => notification.DeletedAt == null);
        });
    }

    private static void ConfigureLike(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Like>(entity =>
        {
            entity.ToTable("likes");

            entity.HasKey(like => like.Id);

            entity.Property(like => like.Id)
                .HasColumnName("id");

            entity.Property(like => like.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(like => like.PostId)
                .HasColumnName("post_id")
                .IsRequired();

            entity.Property(like => like.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(like => like.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(like => like.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasOne(like => like.Post)
                .WithMany(post => post.Likes)
                .HasForeignKey(like => like.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(like => like.User)
                .WithMany()
                .HasForeignKey(like => like.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(like => new
            {
                like.UserId,
                like.PostId
            })
                .IsUnique()
                .HasDatabaseName("ux_likes_user_post");

            entity.HasIndex(like => like.PostId)
                .HasDatabaseName("ix_likes_post_id");
        });
    }

    private static void ConfigureComment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");

            entity.HasKey(comment => comment.Id);

            entity.Property(comment => comment.Id)
                .HasColumnName("id");

            entity.Property(comment => comment.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(comment => comment.PostId)
                .HasColumnName("post_id")
                .IsRequired();

            entity.Property(comment => comment.Content)
                .HasColumnName("content")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(comment => comment.DeletedReason)
                .HasColumnName("deleted_reason")
                .HasMaxLength(150);

            entity.Property(comment => comment.DeletedById)
                .HasColumnName("deleted_by_id");

            entity.Property(comment => comment.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(comment => comment.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(comment => comment.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasOne(comment => comment.Post)
                .WithMany(post => post.Comments)
                .HasForeignKey(comment => comment.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(comment => comment.User)
                .WithMany()
                .HasForeignKey(comment => comment.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(comment => comment.DeletedBy)
                .WithMany(account => account.DeletedComments)
                .HasForeignKey(comment => comment.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(comment => new
            {
                comment.PostId,
                comment.CreatedAt
            })
                .HasDatabaseName("ix_comments_post_created");

            entity.HasIndex(comment => new
            {
                comment.UserId,
                comment.PostId,
                comment.CreatedAt
            })
                .HasDatabaseName("ix_comments_user_post_created");
        });
    }

    private static void ConfigureShare(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Share>(entity =>
        {
            entity.ToTable("shares");

            entity.HasKey(share => share.Id);

            entity.Property(share => share.Id)
                .HasColumnName("id");

            entity.Property(share => share.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(share => share.PostId)
                .HasColumnName("post_id")
                .IsRequired();

            entity.Property(share => share.Platform)
                .HasColumnName("platform")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(share => share.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(share => share.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(share => share.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasOne(share => share.Post)
                .WithMany(post => post.Shares)
                .HasForeignKey(share => share.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(share => share.User)
                .WithMany()
                .HasForeignKey(share => share.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(share => new
            {
                share.PostId,
                share.CreatedAt
            })
                .HasDatabaseName("ix_shares_post_created");

            entity.HasIndex(share => new
            {
                share.UserId,
                share.PostId
            })
                .HasDatabaseName("ix_shares_user_post");
        });
    }

    private static void ConfigureStory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Story>(entity =>
        {
            entity.ToTable("stories");

            entity.HasKey(story => story.Id);

            entity.Property(story => story.Id)
                .HasColumnName("id");

            entity.Property(story => story.OrganizationId)
                .HasColumnName("organization_id")
                .IsRequired();

            entity.Property(story => story.MediaType)
                .HasColumnName("media_type")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(story => story.MediaUrl)
                .HasColumnName("media_url")
                .HasMaxLength(500);

            entity.Property(story => story.TextContent)
                .HasColumnName("text_content")
                .HasMaxLength(280);

            entity.Property(story => story.IsExpired)
                .HasColumnName("is_expired")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(story => story.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            entity.Property(story => story.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(story => story.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(story => story.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasOne(story => story.Organization)
                .WithMany(organization => organization.Stories)
                .HasForeignKey(story => story.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(story => new
            {
                story.OrganizationId,
                story.IsExpired,
                story.ExpiresAt
            })
                .HasDatabaseName("ix_stories_org_expired_expires_at");

            entity.HasIndex(story => story.ExpiresAt)
                .HasDatabaseName("ix_stories_expires_at");

            entity.HasCheckConstraint(
                "ck_stories_text_or_media_content",
                """
            (
                media_type = 'Text'
                AND text_content IS NOT NULL
                AND media_url IS NULL
            )
            OR
            (
                media_type IN ('Image', 'Video')
                AND media_url IS NOT NULL
            )
            """
            );
        });
    }

    private static void ConfigureStoryView(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoryView>(entity =>
        {
            entity.ToTable("story_views");

            entity.HasKey(storyView => storyView.Id);

            entity.Property(storyView => storyView.Id)
                .HasColumnName("id");

            entity.Property(storyView => storyView.StoryId)
                .HasColumnName("story_id")
                .IsRequired();

            entity.Property(storyView => storyView.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            entity.Property(storyView => storyView.ViewedAt)
                .HasColumnName("viewed_at")
                .HasDefaultValueSql("timezone('utc', now())")
                .IsRequired();

            entity.Property(storyView => storyView.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(storyView => storyView.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(storyView => storyView.DeletedAt)
                .HasColumnName("deleted_at");

            entity.HasOne(storyView => storyView.Story)
                .WithMany(story => story.Views)
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
            })
                .IsUnique()
                .HasDatabaseName("ux_story_views_story_account");

            entity.HasIndex(storyView => storyView.AccountId)
                .HasDatabaseName("ix_story_views_account_id");
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
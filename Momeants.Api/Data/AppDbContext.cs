using Microsoft.EntityFrameworkCore;
using Momeants.Api.Domain;
using System.Text.Json;

namespace Momeants.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Momeant> Momeants => Set<Momeant>();
    public DbSet<MediaObject> MediaObjects => Set<MediaObject>();
    public DbSet<Relationship> Relationships => Set<Relationship>();
    public DbSet<ImportantPerson> ImportantPeople => Set<ImportantPerson>();
    public DbSet<MomeantPerson> MomeantPeople => Set<MomeantPerson>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<FeedItem> FeedItems => Set<FeedItem>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.ClerkUserId).HasColumnName("clerk_user_id").IsRequired();
            e.HasIndex(x => x.ClerkUserId).IsUnique();
            e.Property(x => x.Username).HasColumnName("username");
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.DisplayName).HasColumnName("display_name").IsRequired();
            e.Property(x => x.FullName).HasColumnName("full_name");
            e.Property(x => x.AvatarMediaId).HasColumnName("avatar_media_id");
            e.Property(x => x.BirthDate).HasColumnName("birth_date");
            e.Property(x => x.City).HasColumnName("city");
            e.Property(x => x.Region).HasColumnName("region");
            e.Property(x => x.Country).HasColumnName("country");
            e.Property(x => x.Bio).HasColumnName("bio");
            e.Property(x => x.AccountStatus).HasColumnName("account_status").HasDefaultValue("active");
            e.Property(x => x.ProfileVisibility).HasColumnName("profile_visibility").HasDefaultValue("friends");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        });

        // Momeants
        modelBuilder.Entity<Momeant>(e =>
        {
            e.ToTable("momeants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.OwnerUserId).HasColumnName("owner_user_id");
            e.Property(x => x.PrimaryMediaId).HasColumnName("primary_media_id");
            e.Property(x => x.Caption).HasColumnName("caption");
            e.Property(x => x.CapturedAt).HasColumnName("captured_at");
            e.Property(x => x.PostedAt).HasColumnName("posted_at").HasDefaultValueSql("now()");
            e.Property(x => x.LocationLabel).HasColumnName("location_label");
            e.Property(x => x.LocationLat).HasColumnName("location_lat").HasPrecision(9, 6);
            e.Property(x => x.LocationLng).HasColumnName("location_lng").HasPrecision(9, 6);
            e.Property(x => x.AudienceType).HasColumnName("audience_type").HasDefaultValue("friends");
            e.Property(x => x.SignificanceScore).HasColumnName("significance_score").HasPrecision(8, 4).HasDefaultValue(0m);
            e.Property(x => x.MemoryScore).HasColumnName("memory_score").HasPrecision(8, 4).HasDefaultValue(0m);
            e.Property(x => x.CommentsEnabled).HasColumnName("comments_enabled").HasDefaultValue(true);
            e.Property(x => x.ResharingAllowed).HasColumnName("resharing_allowed").HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            e.HasIndex(x => new { x.OwnerUserId, x.PostedAt });
            e.HasIndex(x => new { x.AudienceType, x.PostedAt });
            e.HasOne(x => x.Owner).WithMany(u => u.Momeants).HasForeignKey(x => x.OwnerUserId);
        });

        // MediaObjects
        modelBuilder.Entity<MediaObject>(e =>
        {
            e.ToTable("media_objects");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.OwnerUserId).HasColumnName("owner_user_id");
            e.Property(x => x.R2Bucket).HasColumnName("r2_bucket");
            e.Property(x => x.R2ObjectKey).HasColumnName("r2_object_key");
            e.HasIndex(x => x.R2ObjectKey).IsUnique();
            e.Property(x => x.MediaType).HasColumnName("media_type");
            e.Property(x => x.MimeType).HasColumnName("mime_type");
            e.Property(x => x.ByteSize).HasColumnName("byte_size");
            e.Property(x => x.Width).HasColumnName("width");
            e.Property(x => x.Height).HasColumnName("height");
            e.Property(x => x.DurationMs).HasColumnName("duration_ms");
            e.Property(x => x.Sha256Hex).HasColumnName("sha256_hex");
            e.Property(x => x.Blurhash).HasColumnName("blurhash");
            e.Property(x => x.ModerationStatus).HasColumnName("moderation_status").HasDefaultValue("pending");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            e.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerUserId);
        });

        // Relationships
        modelBuilder.Entity<Relationship>(e =>
        {
            e.ToTable("relationships");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.RequesterUserId).HasColumnName("requester_user_id");
            e.Property(x => x.AddresseeUserId).HasColumnName("addressee_user_id");
            e.Property(x => x.Status).HasColumnName("status").HasDefaultValue("pending");
            e.Property(x => x.RelationshipLabel).HasColumnName("relationship_label");
            e.Property(x => x.ClosenessScore).HasColumnName("closeness_score").HasPrecision(8, 4).HasDefaultValue(0m);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => new { x.RequesterUserId, x.AddresseeUserId }).IsUnique();
            e.HasIndex(x => new { x.RequesterUserId, x.Status });
            e.HasIndex(x => new { x.AddresseeUserId, x.Status });
            e.HasOne(x => x.Requester).WithMany().HasForeignKey(x => x.RequesterUserId);
            e.HasOne(x => x.Addressee).WithMany().HasForeignKey(x => x.AddresseeUserId);
        });

        // ImportantPeople
        modelBuilder.Entity<ImportantPerson>(e =>
        {
            e.ToTable("important_people");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.PersonUserId).HasColumnName("person_user_id");
            e.Property(x => x.DisplayName).HasColumnName("display_name");
            e.Property(x => x.Category).HasColumnName("category");
            e.Property(x => x.CustomLabel).HasColumnName("custom_label");
            e.Property(x => x.Priority).HasColumnName("priority").HasDefaultValue(0);
            e.Property(x => x.NotesPrivate).HasColumnName("notes_private");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasOne(x => x.PersonUser).WithMany().HasForeignKey(x => x.PersonUserId);
        });

        // MomeantPeople
        modelBuilder.Entity<MomeantPerson>(e =>
        {
            e.ToTable("momeant_people");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.MomeantId).HasColumnName("momeant_id");
            e.Property(x => x.PersonUserId).HasColumnName("person_user_id");
            e.Property(x => x.DisplayName).HasColumnName("display_name");
            e.Property(x => x.RelationshipToOwner).HasColumnName("relationship_to_owner");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.HasOne(x => x.Momeant).WithMany(m => m.TaggedPeople).HasForeignKey(x => x.MomeantId);
            e.HasOne(x => x.PersonUser).WithMany().HasForeignKey(x => x.PersonUserId);
        });

        // Reactions
        modelBuilder.Entity<Reaction>(e =>
        {
            e.ToTable("reactions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.MomeantId).HasColumnName("momeant_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.ReactionType).HasColumnName("reaction_type");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.HasIndex(x => new { x.MomeantId, x.UserId, x.ReactionType }).IsUnique();
            e.HasIndex(x => x.MomeantId);
            e.HasOne(x => x.Momeant).WithMany(m => m.Reactions).HasForeignKey(x => x.MomeantId);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        // Comments
        modelBuilder.Entity<Comment>(e =>
        {
            e.ToTable("comments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.MomeantId).HasColumnName("momeant_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Body).HasColumnName("body");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            e.HasIndex(x => new { x.MomeantId, x.CreatedAt });
            e.HasOne(x => x.Momeant).WithMany(m => m.Comments).HasForeignKey(x => x.MomeantId);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        // FeedItems
        modelBuilder.Entity<FeedItem>(e =>
        {
            e.ToTable("feed_items");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.MomeantId).HasColumnName("momeant_id");
            e.Property(x => x.SourceType).HasColumnName("source_type");
            e.Property(x => x.RankScore).HasColumnName("rank_score").HasPrecision(12, 6).HasDefaultValue(0m);
            e.Property(x => x.ReasonCode).HasColumnName("reason_code");
            e.Property(x => x.InsertedAt).HasColumnName("inserted_at").HasDefaultValueSql("now()");
            e.Property(x => x.SeenAt).HasColumnName("seen_at");
            e.Property(x => x.DismissedAt).HasColumnName("dismissed_at");
            e.HasIndex(x => new { x.UserId, x.MomeantId }).IsUnique();
            e.HasIndex(x => new { x.UserId, x.RankScore });
            e.HasIndex(x => new { x.UserId, x.SeenAt });
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Momeant).WithMany().HasForeignKey(x => x.MomeantId);
        });

        // Notifications
        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
            e.Property(x => x.Type).HasColumnName("type");
            e.Property(x => x.Title).HasColumnName("title");
            e.Property(x => x.Body).HasColumnName("body");
            e.Property(x => x.DeepLink).HasColumnName("deep_link");
            e.Property(x => x.ReadAt).HasColumnName("read_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.HasIndex(x => new { x.UserId, x.ReadAt, x.CreatedAt });
            e.HasOne(x => x.User).WithMany(u => u.Notifications).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId);
        });

        // Reports
        modelBuilder.Entity<Report>(e =>
        {
            e.ToTable("reports");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.ReporterUserId).HasColumnName("reporter_user_id");
            e.Property(x => x.TargetType).HasColumnName("target_type");
            e.Property(x => x.TargetId).HasColumnName("target_id");
            e.Property(x => x.Reason).HasColumnName("reason");
            e.Property(x => x.Details).HasColumnName("details");
            e.Property(x => x.Status).HasColumnName("status").HasDefaultValue("open");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.ResolvedAt).HasColumnName("resolved_at");
            e.HasOne(x => x.Reporter).WithMany().HasForeignKey(x => x.ReporterUserId);
        });

        // AuditEvents
        modelBuilder.Entity<AuditEvent>(e =>
        {
            e.ToTable("audit_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.EventType).HasColumnName("event_type");
            e.Property(x => x.EntityType).HasColumnName("entity_type");
            e.Property(x => x.EntityId).HasColumnName("entity_id");
            e.Property(x => x.IpAddress).HasColumnName("ip_address");
            e.Property(x => x.UserAgent).HasColumnName("user_agent");
            e.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new());
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        // RefreshTokens
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.TokenHash).HasColumnName("token_hash");
            e.Property(x => x.DeviceId).HasColumnName("device_id");
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.RevokedAt).HasColumnName("revoked_at");
            e.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using SmartTicket.Domain.Entities;

namespace SmartTicket.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketEvent> TicketEvents => Set<TicketEvent>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<CommentAttachment> CommentAttachments => Set<CommentAttachment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users");
            b.HasKey(x => x.Id);

            b.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);

            b.HasIndex(x => x.Email)
                .IsUnique();

            b.Property(x => x.PasswordHash)
                .IsRequired();

            b.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(50);

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.Property(x => x.FailedLoginCount)
                .IsRequired()
                .HasDefaultValue(0);

            b.Property(x => x.LockoutUntil);

            b.HasMany(x => x.RefreshTokens)
                .WithOne() 
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("RefreshTokens");
            b.HasKey(x => x.Id);

            b.Property(x => x.UserId).IsRequired();

            b.Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            b.HasIndex(x => x.TokenHash)
                .IsUnique();

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.Property(x => x.ExpiresAt)
                .IsRequired();

            b.Property(x => x.RevokedAt);

            b.Property(x => x.ReplacedByTokenHash)
                .HasMaxLength(128);


            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.ExpiresAt);
        });

        modelBuilder.Entity<Ticket>(b =>
        {
            b.ToTable("Tickets");
            b.HasKey(x => x.Id);

            b.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(4000);

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.Property(x => x.CreatedByUserId)
                .IsRequired();

            b.Property(x => x.Status)
                .IsRequired();

            b.Property(x => x.Priority)
                .IsRequired();

            b.Property(x => x.DueAt)
                .HasColumnType("datetime2");

            b.Property(x => x.ClosedAt);
            b.Property(x => x.AssignedToUserId);
            b.Property(x => x.AssignedAt);

            b.Property(x => x.RowVersion)
                .IsRowVersion();

            b.HasIndex(x => x.CreatedByUserId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.AssignedToUserId);
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => new { x.CreatedByUserId, x.CreatedAt })
                .IsDescending(false, true);
            b.HasIndex(x => new { x.AssignedToUserId, x.AssignedAt })
                .IsDescending(false, true);

            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TicketComment>(b =>
        {
            b.ToTable("TicketComments");
            b.HasKey(x => x.Id);

            b.Property(x => x.TicketId)
                .IsRequired();

            b.Property(x => x.AuthorUserId)
                .IsRequired();

            b.Property(x => x.Text)
                .IsRequired()
                .HasMaxLength(2000);

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.HasIndex(x => new { x.TicketId, x.CreatedAt })
                .IsDescending(false, true);

            b.HasOne<Ticket>()
                .WithMany()
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.AuthorUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TicketAttachment>(b =>
        {
            b.ToTable("TicketAttachments");
            b.HasKey(x => x.Id);

            b.Property(x => x.TicketId)
                .IsRequired();

            b.Property(x => x.Url)
                .IsRequired()
                .HasMaxLength(4000);

            b.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(256);

            b.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.Size)
                .IsRequired();

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.HasIndex(x => new { x.TicketId, x.CreatedAt })
                .IsDescending(false, true);

            b.HasOne<Ticket>()
                .WithMany()
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommentAttachment>(b =>
        {
            b.ToTable("CommentAttachments");
            b.HasKey(x => x.Id);

            b.Property(x => x.CommentId)
                .IsRequired();

            b.Property(x => x.Url)
                .IsRequired()
                .HasMaxLength(4000);

            b.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(256);

            b.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.Size)
                .IsRequired();

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.HasIndex(x => new { x.CommentId, x.CreatedAt })
                .IsDescending(false, true);

            b.HasOne<TicketComment>()
                .WithMany()
                .HasForeignKey(x => x.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TicketEvent>(b =>
        {
            b.ToTable("TicketEvents");
            b.HasKey(x => x.Id);

            b.Property(x => x.TicketId)
                .IsRequired();

            b.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(80);

            b.Property(x => x.ActorUserId)
                .IsRequired(false);

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.Property(x => x.DataJson)
                .HasMaxLength(4000);

            b.HasIndex(x => new { x.TicketId, x.CreatedAt })
                .IsDescending(false, true);

            b.HasOne<Ticket>()
                .WithMany()
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IdempotencyKey>(b =>
        {
            b.ToTable("IdempotencyKeys");
            b.HasKey(x => x.Id);

            b.Property(x => x.UserId)
                .IsRequired();

            b.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.Path)
                .IsRequired()
                .HasMaxLength(256);

            b.Property(x => x.Method)
                .IsRequired()
                .HasMaxLength(16);

            b.Property(x => x.StatusCode)
                .IsRequired();

            b.Property(x => x.ResponseBodyJson)
                .HasMaxLength(4000);

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.Property(x => x.ExpiresAt)
                .IsRequired();

            b.HasIndex(x => new { x.UserId, x.Key, x.Path, x.Method })
                .IsUnique();
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(x => x.Id);

            b.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.PayloadJson)
                .IsRequired()
                .HasMaxLength(4000);

            b.Property(x => x.OccurredAt)
                .IsRequired();

            b.Property(x => x.ProcessedAt);

            b.Property(x => x.Error)
                .HasMaxLength(4000);

            b.HasIndex(x => x.ProcessedAt);
            b.HasIndex(x => x.OccurredAt);
        });

        modelBuilder.Entity<AuditEvent>(b =>
        {
            b.ToTable("AuditEvents");
            b.HasKey(x => x.Id);

            b.Property(x => x.Category).IsRequired().HasMaxLength(50);
            b.Property(x => x.EventType).IsRequired().HasMaxLength(80);

            b.Property(x => x.SubjectType).HasMaxLength(50);
            b.Property(x => x.IpAddress).HasMaxLength(64);
            b.Property(x => x.CorrelationId).HasMaxLength(64);
            b.Property(x => x.TraceId).HasMaxLength(64);

            b.Property(x => x.DataJson).HasMaxLength(4000);
            b.Property(x => x.Message).HasMaxLength(400);

            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.Category);
            b.HasIndex(x => x.EventType);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.SubjectType, x.SubjectId });
        });

    }
}

using IFMS.Station.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IFMS.Station.Infrastructure.Persistence;

public class StationDbContext : DbContext
{
    public StationDbContext(DbContextOptions<StationDbContext> options) : base(options) { }
    
    public DbSet<Domain.Entities.Station> Stations => Set<Domain.Entities.Station>();
    public DbSet<DealerAssignment> DealerAssignments => Set<DealerAssignment>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Station configuration
        modelBuilder.Entity<Domain.Entities.Station>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.ToTable("Stations");
            
            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(s => s.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.HasIndex(s => s.LicenseNumber)
                .IsUnique();
            
            entity.Property(s => s.City)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(s => s.State)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(s => s.Latitude)
                .HasPrecision(9, 6);
            
            entity.Property(s => s.Longitude)
                .HasPrecision(9, 6);
            
            entity.Property(s => s.IsActive)
                .HasDefaultValue(true);
            
            // One-to-one relationship with DealerAssignment
            entity.HasOne(s => s.DealerAssignment)
                .WithOne(da => da.Station)
                .HasForeignKey<DealerAssignment>(da => da.StationId);
        });
        
        // DealerAssignment configuration
        modelBuilder.Entity<DealerAssignment>(entity =>
        {
            entity.HasKey(da => da.Id);
            entity.ToTable("DealerAssignments");
            
            entity.Property(da => da.UserId)
                .IsRequired();
            
            entity.HasIndex(da => da.StationId)
                .IsUnique();
        });

        // StaffMember configuration
        modelBuilder.Entity<StaffMember>(entity =>
        {
            entity.HasKey(sm => sm.Id);
            entity.ToTable("StaffMembers");

            entity.Property(sm => sm.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(sm => sm.Role)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(sm => sm.Shift)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(sm => sm.Phone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(sm => sm.Email)
                .HasMaxLength(200);

            entity.Property(sm => sm.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.Property(sm => sm.JoinDate)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(sm => sm.StationId);

            entity.HasOne(sm => sm.Station)
                .WithMany()
                .HasForeignKey(sm => sm.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Domain.Entities.Station>();
        
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(Domain.Entities.Station.CreatedAt)).CurrentValue = DateTime.UtcNow;
                entry.Property(nameof(Domain.Entities.Station.UpdatedAt)).CurrentValue = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(Domain.Entities.Station.UpdatedAt)).CurrentValue = DateTime.UtcNow;
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}

using System;
using Microsoft.EntityFrameworkCore;

public class ExternalAttendanceDbContext : DbContext
{
    public ExternalAttendanceDbContext(DbContextOptions<ExternalAttendanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExternalStudent> ExternalStudents { get; set; }

    public DbSet<ExternalAttendanceEvent> ExternalAttendanceEvents { get; set; }

    public DbSet<EasyAttendanceEvent> EasyAttendanceEvents { get; set; }

    public DbSet<ExternalEmployee> ExternalEmployees { get; set; }

    public DbSet<DigiStaffAttendanceEvent> DigiStaffAttendanceEvents { get; set; }

    // New external attendance receiver tables.
    public DbSet<School> SchoolsList { get; set; }

    public DbSet<AttendanceEvent> AttendanceEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExternalStudent>()
            .HasIndex(x => x.StudentNumber)
            .IsUnique();

        modelBuilder.Entity<ExternalAttendanceEvent>()
            .HasIndex(x => x.EventId)
            .IsUnique();

        modelBuilder.Entity<ExternalAttendanceEvent>()
            .HasIndex(x => x.StudentNumber);

        modelBuilder.Entity<ExternalAttendanceEvent>()
            .HasIndex(x => x.ResultStatus);

        modelBuilder.Entity<EasyAttendanceEvent>()
            .HasIndex(x => x.StaffId);

        modelBuilder.Entity<EasyAttendanceEvent>()
            .HasIndex(x => x.ReceivedAt);

        modelBuilder.Entity<ExternalEmployee>()
            .HasIndex(x => x.StaffId)
            .IsUnique();

        modelBuilder.Entity<DigiStaffAttendanceEvent>()
            .HasIndex(x => x.EmployeeId);

        modelBuilder.Entity<DigiStaffAttendanceEvent>()
            .HasIndex(x => x.ReceivedAt);

        // Store SchoolCode as the primary key and link received events back to the school.
        // SchoolName on AttendanceEvent is a snapshot, copied when each punch arrives.
        modelBuilder.Entity<School>(entity =>
        {
            entity.ToTable("SchoolsList");
            entity.HasKey(x => x.SchoolCode);
        });

        modelBuilder.Entity<AttendanceEvent>(entity =>
        {
            entity.ToTable("AttendanceEvents");
            entity.HasKey(x => x.Id);
            // C# uses ClassName because `class` is a reserved keyword; SQL uses [Class].
            entity.Property(x => x.ClassName).HasColumnName("Class");
            entity.HasOne<School>()
                .WithMany()
                .HasForeignKey(x => x.SchoolCode)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.SchoolCode, x.PunchTime });
        });

        var seedDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        modelBuilder.Entity<ExternalStudent>().HasData(
            new ExternalStudent
            {
                Id = 1,
                StudentNumber = "AA-82-1",
                FullName = "Test Student One",
                ClassName = "AVM",
                RollNumber = "01",
                IsActive = true,
                ResponseMode = "ACCEPTED",
                CreatedOn = seedDate
            },
            new ExternalStudent
            {
                Id = 2,
                StudentNumber = "AS-82-2",
                FullName = "Test Student Two",
                ClassName = "AVM",
                RollNumber = "02",
                IsActive = true,
                ResponseMode = "REJECTED",
                CreatedOn = seedDate
            },
            new ExternalStudent
            {
                Id = 3,
                StudentNumber = "AH-82-3",
                FullName = "Test Student Three",
                ClassName = "AVM",
                RollNumber = "03",
                IsActive = true,
                ResponseMode = "DUPLICATED",
                CreatedOn = seedDate
            },
            new ExternalStudent
            {
                Id = 4,
                StudentNumber = "AD-82-4",
                FullName = "Test Student Four",
                ClassName = "Class One",
                RollNumber = "04",
                IsActive = false,
                ResponseMode = "REJECTED",
                CreatedOn = seedDate
            }
        );
    }
}

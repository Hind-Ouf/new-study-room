using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using StudyRoomBooking.Models;

namespace StudyRoomBooking.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Course> Courses => Set<Course>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Cascade); // deleting a room removes its bookings

            modelBuilder.Entity<Booking>()
                .HasMany(b => b.Courses)
                .WithMany(c => c.Bookings)
                .UsingEntity<Dictionary<string, object>>(
                    "BookingCourse",
                    right => right.HasOne<Course>().WithMany().HasForeignKey("CourseId").OnDelete(DeleteBehavior.Cascade),
                    left => left.HasOne<Booking>().WithMany().HasForeignKey("BookingId").OnDelete(DeleteBehavior.Cascade),
                    join => join.HasKey("BookingId", "CourseId"));

            modelBuilder.Entity<Course>().HasIndex(c => c.Code).IsUnique();

            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, Name = "A101", Floor = 1, Capacity = 4, HasProjector = false },
                new Room { Id = 2, Name = "A102", Floor = 1, Capacity = 8, HasProjector = true },
                new Room { Id = 3, Name = "B201", Floor = 2, Capacity = 6, HasProjector = false },
                new Room { Id = 4, Name = "B202", Floor = 2, Capacity = 12, HasProjector = true });

            modelBuilder.Entity<Course>().HasData(
                new Course { Id = 1, Code = "ITPE3200", Name = "Web Development" },
                new Course { Id = 2, Code = "DATA1500", Name = "Introduction to Programming" },
                new Course { Id = 3, Code = "ITPE3100", Name = "Software Engineering" });
        }
    }
}

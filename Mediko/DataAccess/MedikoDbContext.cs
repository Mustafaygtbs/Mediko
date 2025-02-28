using Mediko.Entities;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Mediko.DataAccess
{
    public class MedikoDbContext : IdentityDbContext<User>
    {
        public MedikoDbContext(DbContextOptions<MedikoDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Policlinic> Policlinics { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<PoliclinicTimeslot> PoliclinicTimeslots { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //    User silindiğinde Appointment da silinir.
            builder.Entity<Appointment>()
       .HasOne(a => a.User)
       .WithMany()
       .HasForeignKey(a => a.UserId)
       .OnDelete(DeleteBehavior.Cascade);

            //  Timeslot silinince Appointment silinmez
            builder.Entity<Appointment>()
       .HasOne(a => a.PoliclinicTimeslot)
       .WithMany()
       .HasForeignKey(a => a.PoliclinicTimeslotId)
       .OnDelete(DeleteBehavior.Restrict);

             //    Poliklinic silinince Appointment doğrudan silinmesin
            builder.Entity<Appointment>()
        .HasOne(a => a.Policlinic)
        .WithMany()
        .HasForeignKey(a => a.PoliclinicId)
        .OnDelete(DeleteBehavior.Restrict);

            //    Poliklinic silinince Timeslot da silinsin.
            builder.Entity<PoliclinicTimeslot>()
       .HasOne(ts => ts.Policlinic)
       .WithMany()
       .HasForeignKey(ts => ts.PoliclinicId)
       .OnDelete(DeleteBehavior.Cascade);



            builder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<PoliclinicTimeslot>()
              .HasIndex(ts => new { ts.PoliclinicId, ts.Date, ts.StartTime })
              .IsUnique();

            builder.Entity<Appointment>()
        .Property(a => a.Status)
        .HasConversion(
            v => v.ToString(),
            v => (AppointmentStatus)Enum.Parse(typeof(AppointmentStatus), v));

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = config.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }

        }
    }
}

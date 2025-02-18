using Mediko.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mediko.DataAccess
{
    public class MedikoDbContext : IdentityDbContext
    {
        public MedikoDbContext(DbContextOptions<MedikoDbContext> options)
            : base(options)
        {
        }
         public DbSet<User> Kullanicilar { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Policlinic> Policlinics { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Doctor>()
               .HasOne(d => d.Policlinic)
               .WithMany(p => p.Doctors)
               .HasForeignKey(d => d.PoliclinicId)
               .OnDelete(DeleteBehavior.Restrict); 

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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

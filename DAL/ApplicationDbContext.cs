using DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<FollowUpForm> FollowUpForms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Donor>()
                .HasMany(e => e.Requests)
                .WithOne(e => e.Donor)
                .HasForeignKey(e => e.DonorId)
                .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<Patient>()
                .HasMany(e => e.Requests)
                .WithOne(e => e.Patient)
                .HasForeignKey(e => e.PatientId)
                .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<Donor>()
                .HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Donor>(d => d.UserId)
                .HasPrincipalKey<ApplicationUser>(d => d.Id);

            modelBuilder.Entity<Patient>()
                 .HasOne(e => e.User)
                 .WithOne()
                 .HasForeignKey<Patient>(d => d.UserId)
                 .HasPrincipalKey<ApplicationUser>(d => d.Id);

            modelBuilder.Entity<Hospital>(entity =>
            {
                entity.Property(e => e.HospitalName).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.Governorate).IsRequired();
                entity.Property(e => e.Province).IsRequired();
                entity.Property(e => e.Address).IsRequired();

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId);
            });
        }
    }
}

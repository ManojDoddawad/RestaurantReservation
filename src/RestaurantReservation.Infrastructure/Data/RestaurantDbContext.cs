using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Infrastructure.Data;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<MenuCategory> MenuCategories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TimeSlot> TimeSlots { get; set; }
    public DbSet<ReservationLog> ReservationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CustomerId).HasColumnName("customerid");
            entity.Property(e => e.FirstName).HasColumnName("firstname").HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasColumnName("lastname").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
            entity.Property(e => e.DateOfBirth).HasColumnName("dateofbirth");
            entity.Property(e => e.DietaryRestrictions).HasColumnName("dietaryrestrictions").HasMaxLength(500);
            entity.Property(e => e.IsVIP).HasColumnName("isvip").HasDefaultValue(false);
            entity.Property(e => e.IsBlacklisted).HasColumnName("isblacklisted").HasDefaultValue(false);
            entity.Property(e => e.CreatedDate).HasColumnName("createddate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedDate).HasColumnName("modifieddate").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Table configuration
        modelBuilder.Entity<Table>(entity =>
        {
            entity.ToTable("tables");
            entity.HasKey(e => e.TableId);
            entity.Property(e => e.TableId).HasColumnName("tableid");
            entity.Property(e => e.TableNumber).HasColumnName("tablenumber").HasMaxLength(10).IsRequired();
            entity.Property(e => e.Capacity).HasColumnName("capacity").IsRequired();
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(50);
            entity.Property(e => e.IsActive).HasColumnName("isactive").HasDefaultValue(true);
            entity.Property(e => e.MinimumCapacity).HasColumnName("minimumcapacity");
            entity.Property(e => e.MaximumCapacity).HasColumnName("maximumcapacity");
            entity.Property(e => e.CreatedDate).HasColumnName("createddate").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.TableNumber).IsUnique();
        });

        // Reservation configuration
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("reservations");
            entity.HasKey(e => e.ReservationId);
            entity.Property(e => e.ReservationId).HasColumnName("reservationid");
            entity.Property(e => e.CustomerId).HasColumnName("customerid");
            entity.Property(e => e.TableId).HasColumnName("tableid");
            entity.Property(e => e.ReservationDate).HasColumnName("reservationdate").IsRequired();
            entity.Property(e => e.PartySize).HasColumnName("partysize").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            entity.Property(e => e.SpecialRequests).HasColumnName("specialrequests").HasMaxLength(1000);
            entity.Property(e => e.Duration).HasColumnName("duration").HasDefaultValue(120);
            entity.Property(e => e.IsConfirmed).HasColumnName("isconfirmed").HasDefaultValue(false);
            entity.Property(e => e.ConfirmationCode).HasColumnName("confirmationcode").HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasColumnName("createddate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedDate).HasColumnName("modifieddate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CancelledDate).HasColumnName("cancelleddate");
            entity.Property(e => e.CancellationReason).HasColumnName("cancellationreason").HasMaxLength(500);

            entity.HasIndex(e => e.ConfirmationCode).IsUnique();

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Reservations)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(e => e.TableId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MenuCategory configuration
        modelBuilder.Entity<MenuCategory>(entity =>
        {
            entity.ToTable("menucategories");
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryId).HasColumnName("categoryid");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.DisplayOrder).HasColumnName("displayorder");
            entity.Property(e => e.IsActive).HasColumnName("isactive").HasDefaultValue(true);
        });

        // MenuItem configuration
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.ToTable("menuitems");
            entity.HasKey(e => e.MenuItemId);
            entity.Property(e => e.MenuItemId).HasColumnName("menuitemid");
            entity.Property(e => e.CategoryId).HasColumnName("categoryid");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.IsAvailable).HasColumnName("isavailable").HasDefaultValue(true);
            entity.Property(e => e.DietaryTags).HasColumnName("dietarytags").HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasColumnName("imageurl").HasMaxLength(500);
            entity.Property(e => e.PreparationTime).HasColumnName("preparationtime");
            entity.Property(e => e.CreatedDate).HasColumnName("createddate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedDate).HasColumnName("modifieddate").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("passwordhash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("isactive").HasDefaultValue(true);
            entity.Property(e => e.CreatedDate).HasColumnName("createddate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastLoginDate).HasColumnName("lastlogindate");

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // TimeSlot configuration
        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.ToTable("timeslots");
            entity.HasKey(e => e.TimeSlotId);
            entity.Property(e => e.TimeSlotId).HasColumnName("timeslotid");
            entity.Property(e => e.DayOfWeek).HasColumnName("dayofweek").IsRequired();
            entity.Property(e => e.StartTime).HasColumnName("starttime").IsRequired();
            entity.Property(e => e.EndTime).HasColumnName("endtime").IsRequired();
            entity.Property(e => e.MaxReservations).HasColumnName("maxreservations");
            entity.Property(e => e.IsActive).HasColumnName("isactive").HasDefaultValue(true);
        });

        // ReservationLog configuration
        modelBuilder.Entity<ReservationLog>(entity =>
        {
            entity.ToTable("reservationlog");
            entity.HasKey(e => e.LogId);
            entity.Property(e => e.LogId).HasColumnName("logid");
            entity.Property(e => e.ReservationId).HasColumnName("reservationid");
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
            entity.Property(e => e.OldStatus).HasColumnName("oldstatus").HasMaxLength(20);
            entity.Property(e => e.NewStatus).HasColumnName("newstatus").HasMaxLength(20);
            entity.Property(e => e.ChangedBy).HasColumnName("changedby");
            entity.Property(e => e.ChangedDate).HasColumnName("changeddate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(1000);

            entity.HasOne(e => e.Reservation)
                .WithMany(r => r.ReservationLogs)
                .HasForeignKey(e => e.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ChangedByUser)
                .WithMany(u => u.ReservationLogs)
                .HasForeignKey(e => e.ChangedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
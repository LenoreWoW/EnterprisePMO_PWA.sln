// <auto-generated />
using System;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EnterprisePMO_PWA.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250401132900_AddSupabaseIntegration")]
    partial class AddSupabaseIntegration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("EnterprisePMO_PWA.Domain.Entities.AuditLog", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Action")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("ChangeSummary")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("EntityId")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("EntityName")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("IpAddress")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<DateTime>("Timestamp")
                    .HasColumnType("datetime2");

                b.Property<Guid?>("UserId")
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Username")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.HasKey("Id");

                b.HasIndex("EntityId");

                b.HasIndex("EntityName");

                b.HasIndex("Timestamp");

                b.HasIndex("UserId");

                b.ToTable("AuditLogs");
            });

            modelBuilder.Entity("EnterprisePMO_PWA.Domain.Entities.ChangeRequest", b =>
            {
                // Entity properties...
            });

            // Other entity configurations...

            modelBuilder.Entity("EnterprisePMO_PWA.Domain.Entities.User", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("CreatedDate")
                    .HasColumnType("datetime2");

                b.Property<Guid?>("DepartmentId")
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Email")
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<string>("FirstName")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<bool>("IsActive")
                    .HasColumnType("bit");

                b.Property<string>("LastName")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<DateTime?>("LastUpdated")
                    .HasColumnType("datetime2");

                b.Property<int>("Role")
                    .HasColumnType("int");

                b.Property<string>("SupabaseId")
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.Property<string>("Username")
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                b.HasKey("Id");

                b.HasIndex("DepartmentId");

                b.HasIndex("SupabaseId");

                b.HasIndex("Username")
                    .IsUnique();

                b.ToTable("Users");
            });

            // Foreign key relationships...
        }
    }
}
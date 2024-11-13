﻿// <auto-generated />
using System;
using DATN.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DATN.Server.Migrations
{
    [DbContext(typeof(AppDBContext))]
<<<<<<<< HEAD:DATN/DATN/Server/Migrations/20241031022926_Init.Designer.cs
    [Migration("20241031022926_Init")]
    partial class Init
========
    [Migration("20241023140535_v1")]
    partial class v1
>>>>>>>> 13dac13dcb52cf853e5229cc6ff35594f19dfcea:DATN/DATN/Server/Migrations/20241023140535_v1.Designer.cs
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.17")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DATN.Shared.Account", b =>
                {
                    b.Property<int>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AccountType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("char(40)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("AccountId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("DATN.Shared.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CategoryDescription")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.HasKey("CategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("DATN.Shared.Customer", b =>
                {
                    b.Property<int>("CustomerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("varchar(12)");

                    b.Property<int>("TotalRewardPoint")
                        .HasColumnType("int");

                    b.HasKey("CustomerId");

                    b.HasIndex("AccountId")
                        .IsUnique();

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("DATN.Shared.CustomerVoucher", b =>
                {
                    b.Property<int>("CustomerVoucherId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("bit");

                    b.Property<DateTime>("RedeemDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("VoucherId")
                        .HasColumnType("int");

                    b.HasKey("CustomerVoucherId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("VoucherId");

                    b.ToTable("CustomerVouchers");
                });

            modelBuilder.Entity("DATN.Shared.Employee", b =>
                {
                    b.Property<int>("EmployeeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("EmployeeName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("HireDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("varchar(12)");

                    b.Property<string>("Position")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("Salary")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("EmployeeId");

                    b.HasIndex("AccountId")
                        .IsUnique();

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("DATN.Shared.EmployeeShifte", b =>
                {
                    b.Property<int>("EmployeeShifteId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("ShifteDay")
                        .HasColumnType("datetime2");

                    b.Property<int>("ShifteId")
                        .HasColumnType("int");

                    b.HasKey("EmployeeShifteId");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("ShifteId");

                    b.ToTable("EmployeeShiftes");
                });

            modelBuilder.Entity("DATN.Shared.Floor", b =>
                {
                    b.Property<int>("FloorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("NumberFloor")
                        .HasColumnType("int");

                    b.HasKey("FloorId");

                    b.ToTable("Floors");
                });

            modelBuilder.Entity("DATN.Shared.Menu", b =>
                {
                    b.Property<int>("MenuId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("MenuDescription")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<byte[]>("MenuImage")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("MenuName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<decimal>("PriceCombo")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("MenuId");

                    b.ToTable("Menus");
                });

            modelBuilder.Entity("DATN.Shared.MenuItem", b =>
                {
                    b.Property<int>("MenuItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("MenuId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.HasKey("MenuItemId");

                    b.HasIndex("MenuId");

                    b.HasIndex("ProductId");

                    b.ToTable("MenuItems");
                });

            modelBuilder.Entity("DATN.Shared.Message", b =>
                {
                    b.Property<int>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("AccountId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("MessageText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Note")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TableId")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("MessageId");

                    b.HasIndex("AccountId");

                    b.HasIndex("TableId");

                    b.ToTable("Message");
                });

            modelBuilder.Entity("DATN.Shared.Order", b =>
                {
                    b.Property<int>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<int?>("CustomerVoucherId")
                        .HasColumnType("int");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Note")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PaymentMethod")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("TableId")
                        .HasColumnType("int");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("OrderId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("CustomerVoucherId")
                        .IsUnique()
                        .HasFilter("[CustomerVoucherId] IS NOT NULL");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("TableId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("DATN.Shared.OrderItem", b =>
                {
                    b.Property<int>("OrderItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("OrderItemId");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderItems");
                });

            modelBuilder.Entity("DATN.Shared.Product", b =>
                {
                    b.Property<int>("ProductId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProductDescription")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<byte[]>("ProductImage")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("UnitId")
                        .HasColumnType("int");

                    b.HasKey("ProductId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("UnitId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("DATN.Shared.Reservation", b =>
                {
                    b.Property<int>("ReservationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("CustomerPhone")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("varchar(12)");

                    b.Property<decimal>("DepositPayment")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPayment")
                        .HasColumnType("bit");

                    b.Property<int>("NumberGuest")
                        .HasColumnType("int");

                    b.Property<int>("NumberSeat")
                        .HasColumnType("int");

                    b.Property<string>("PaymentMethod")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<DateTime>("ReservationDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("TableId")
                        .HasColumnType("int");

                    b.HasKey("ReservationId");

                    b.HasIndex("TableId");

                    b.ToTable("Reservations");
                });

            modelBuilder.Entity("DATN.Shared.RewardPointe", b =>
                {
                    b.Property<int>("RewardPointId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<int>("RewardPoint")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("RewardPointId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("OrderId")
                        .IsUnique();

                    b.ToTable("RewardPointes");
                });

            modelBuilder.Entity("DATN.Shared.Role", b =>
                {
                    b.Property<int>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("RoleDescription")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.HasKey("RoleId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("DATN.Shared.RoleAccount", b =>
                {
                    b.Property<int>("RoleAccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("RoleAccountId");

                    b.HasIndex("AccountId");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleAccounts");
                });

            modelBuilder.Entity("DATN.Shared.Shifte", b =>
                {
                    b.Property<int>("ShifteId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("ShifteName")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.HasKey("ShifteId");

                    b.ToTable("Shiftes");
                });

            modelBuilder.Entity("DATN.Shared.Table", b =>
                {
                    b.Property<int>("TableId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("FloorId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Position")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("SeatingCapacity")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("TableNumber")
                        .HasColumnType("int");

                    b.HasKey("TableId");

                    b.HasIndex("FloorId");

                    b.ToTable("Tables");
                });

            modelBuilder.Entity("DATN.Shared.Unit", b =>
                {
                    b.Property<int>("UnitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("UnitDescription")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("UnitName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("UnitId");

                    b.ToTable("Units");
                });

            modelBuilder.Entity("DATN.Shared.Voucher", b =>
                {
                    b.Property<int>("VoucherId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("DiscountValue")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("ExpriationDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsAcctive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("PointRequired")
                        .HasColumnType("int");

                    b.Property<string>("VoucherCode")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.HasKey("VoucherId");

                    b.ToTable("Vouchers");
                });

            modelBuilder.Entity("DATN.Shared.Customer", b =>
                {
                    b.HasOne("DATN.Shared.Account", "Accounts")
                        .WithOne("Customers")
                        .HasForeignKey("DATN.Shared.Customer", "AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Accounts");
                });

            modelBuilder.Entity("DATN.Shared.CustomerVoucher", b =>
                {
                    b.HasOne("DATN.Shared.Customer", "Customers")
                        .WithMany("CustomerVouchers")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DATN.Shared.Voucher", "Vouchers")
                        .WithMany("CustomerVouchers")
                        .HasForeignKey("VoucherId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Customers");

                    b.Navigation("Vouchers");
                });

            modelBuilder.Entity("DATN.Shared.Employee", b =>
                {
                    b.HasOne("DATN.Shared.Account", "Accounts")
                        .WithOne("Employees")
                        .HasForeignKey("DATN.Shared.Employee", "AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Accounts");
                });

            modelBuilder.Entity("DATN.Shared.EmployeeShifte", b =>
                {
                    b.HasOne("DATN.Shared.Employee", "Employees")
                        .WithMany("EmployeeShiftes")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DATN.Shared.Shifte", "Shiftes")
                        .WithMany("EmployeeShiftes")
                        .HasForeignKey("ShifteId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Employees");

                    b.Navigation("Shiftes");
                });

            modelBuilder.Entity("DATN.Shared.MenuItem", b =>
                {
                    b.HasOne("DATN.Shared.Menu", "Menus")
                        .WithMany("MenuItems")
                        .HasForeignKey("MenuId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DATN.Shared.Product", "Products")
                        .WithMany("MenuItems")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Menus");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("DATN.Shared.Message", b =>
                {
                    b.HasOne("DATN.Shared.Account", "Account")
                        .WithMany("Messages")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN.Shared.Table", "Table")
                        .WithMany("Messages")
                        .HasForeignKey("TableId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Account");

                    b.Navigation("Table");
                });

            modelBuilder.Entity("DATN.Shared.Order", b =>
                {
                    b.HasOne("DATN.Shared.Customer", "Customers")
                        .WithMany("Orders")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DATN.Shared.CustomerVoucher", "CustomerVouchers")
                        .WithOne("Order")
                        .HasForeignKey("DATN.Shared.Order", "CustomerVoucherId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DATN.Shared.Employee", "Employee")
                        .WithMany("Orders")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DATN.Shared.Table", "Tables")
                        .WithMany("Orders")
                        .HasForeignKey("TableId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Customers");

                    b.Navigation("CustomerVouchers");

                    b.Navigation("Employee");

                    b.Navigation("Tables");
                });

            modelBuilder.Entity("DATN.Shared.OrderItem", b =>
                {
                    b.HasOne("DATN.Shared.Order", "Orders")
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DATN.Shared.Product", "Products")
                        .WithMany("OrderItems")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Orders");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("DATN.Shared.Product", b =>
                {
                    b.HasOne("DATN.Shared.Category", "Categories")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DATN.Shared.Unit", "Units")
                        .WithMany("Products")
                        .HasForeignKey("UnitId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Categories");

                    b.Navigation("Units");
                });

            modelBuilder.Entity("DATN.Shared.Reservation", b =>
                {
                    b.HasOne("DATN.Shared.Table", "Tables")
                        .WithMany("Reservations")
                        .HasForeignKey("TableId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Tables");
                });

            modelBuilder.Entity("DATN.Shared.RewardPointe", b =>
                {
                    b.HasOne("DATN.Shared.Customer", "Customers")
                        .WithMany("RewardPoints")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DATN.Shared.Order", "Orders")
                        .WithOne("RewardPointes")
                        .HasForeignKey("DATN.Shared.RewardPointe", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customers");

                    b.Navigation("Orders");
                });

            modelBuilder.Entity("DATN.Shared.RoleAccount", b =>
                {
                    b.HasOne("DATN.Shared.Account", "Accounts")
                        .WithMany("RoleAccounts")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DATN.Shared.Role", "Roles")
                        .WithMany("RoleAccounts")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Accounts");

                    b.Navigation("Roles");
                });

            modelBuilder.Entity("DATN.Shared.Table", b =>
                {
                    b.HasOne("DATN.Shared.Floor", "Floors")
                        .WithMany("Tables")
                        .HasForeignKey("FloorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Floors");
                });

            modelBuilder.Entity("DATN.Shared.Account", b =>
                {
                    b.Navigation("Customers");

                    b.Navigation("Employees");

                    b.Navigation("Messages");

                    b.Navigation("RoleAccounts");
                });

            modelBuilder.Entity("DATN.Shared.Category", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("DATN.Shared.Customer", b =>
                {
                    b.Navigation("CustomerVouchers");

                    b.Navigation("Orders");

                    b.Navigation("RewardPoints");
                });

            modelBuilder.Entity("DATN.Shared.CustomerVoucher", b =>
                {
                    b.Navigation("Order");
                });

            modelBuilder.Entity("DATN.Shared.Employee", b =>
                {
                    b.Navigation("EmployeeShiftes");

                    b.Navigation("Orders");
                });

            modelBuilder.Entity("DATN.Shared.Floor", b =>
                {
                    b.Navigation("Tables");
                });

            modelBuilder.Entity("DATN.Shared.Menu", b =>
                {
                    b.Navigation("MenuItems");
                });

            modelBuilder.Entity("DATN.Shared.Order", b =>
                {
                    b.Navigation("OrderItems");

                    b.Navigation("RewardPointes");
                });

            modelBuilder.Entity("DATN.Shared.Product", b =>
                {
                    b.Navigation("MenuItems");

                    b.Navigation("OrderItems");
                });

            modelBuilder.Entity("DATN.Shared.Role", b =>
                {
                    b.Navigation("RoleAccounts");
                });

            modelBuilder.Entity("DATN.Shared.Shifte", b =>
                {
                    b.Navigation("EmployeeShiftes");
                });

            modelBuilder.Entity("DATN.Shared.Table", b =>
                {
                    b.Navigation("Messages");

                    b.Navigation("Orders");

                    b.Navigation("Reservations");
                });

            modelBuilder.Entity("DATN.Shared.Unit", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("DATN.Shared.Voucher", b =>
                {
                    b.Navigation("CustomerVouchers");
                });
#pragma warning restore 612, 618
        }
    }
}

using DATN.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DATN.Server.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Custom DataType
           
            modelBuilder.Entity<Employee>().Property(d => d.Salary).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Menu>().Property(f => f.PriceCombo).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>().Property(f => f.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>().Property(f => f.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>().Property(f => f.TotalPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(f => f.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Voucher>().Property(f => f.DiscountValue).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Reservation>().Property(f => f.DepositPayment).HasColumnType("decimal(18,2)");

            //Shifte
            modelBuilder.Entity<Shifte>().Property(e => e.Shifte_Name)
                .HasColumnType("nvarchar(15)")
                .HasMaxLength(15)
                .IsRequired();

            modelBuilder.Entity<Shifte>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //Employee
            modelBuilder.Entity<Employee>().Property(e => e.EmployeeName)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Employee>().Property(e => e.PhoneNumber)
                .HasColumnType("varchar(12)")
                .HasMaxLength(12)
                .IsRequired();

            modelBuilder.Entity<Employee>().Property(e => e.Email)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Employee>().Property(e => e.Position)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Employee>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //EmployeeShift
            modelBuilder.Entity<EmployeeShifte>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //Role
            modelBuilder.Entity<Role>().Property(e => e.RoleName)
                .HasColumnType("nvarchar(40)")
                .HasMaxLength(40)
                .IsRequired();

            modelBuilder.Entity<Role>().Property(e => e.RoleDescription)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50);

            modelBuilder.Entity<Role>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //Account
            modelBuilder.Entity<Account>().Property(e => e.AccountType)
                .HasColumnType("nvarchar(20)")
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Account>().Property(e => e.Password)
                .HasColumnType("char(40)")
                .HasMaxLength(40)
                .IsRequired();

            modelBuilder.Entity<Account>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            modelBuilder.Entity<Account>().Property(e => e.IsActive)
                .HasColumnType("bit")
                .IsRequired();

            //RoleAccount
            modelBuilder.Entity<RoleAccount>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //Floor
            modelBuilder.Entity<Floor>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //Table
            modelBuilder.Entity<Table>().Property(e => e.Status)
                .HasColumnType("nvarchar(20)")
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Table>().Property(e => e.Position)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Table>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //Customer
            modelBuilder.Entity<Customer>().Property(e => e.CustomerName)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<Customer>().Property(e => e.PhoneNumber)
                .HasColumnType("varchar(12)")
                .HasMaxLength(12)
                .IsRequired();

            modelBuilder.Entity<Customer>().Property(e => e.Address)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Customer>().Property(e => e.Email)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Customer>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();


            //RewardPoint
            modelBuilder.Entity<RewardPointe>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //Unit
            modelBuilder.Entity<Unit>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            modelBuilder.Entity<Unit>().Property(e => e.UnitName)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<Unit>().Property(e => e.UnitDescription)
                .HasColumnType("nvarchar(255)")
                .HasMaxLength(255);

            //Product

            modelBuilder.Entity<Product>().Property(e => e.ProductName)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Product>().Property(e => e.ProductDescription)
                .HasColumnType("nvarchar(255)")
                .HasMaxLength(255);

            modelBuilder.Entity<Product>().Property(e => e.ProductImage)
                .HasColumnType("varbinary(max)")
                .IsRequired();

            modelBuilder.Entity<Product>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();
            //Category
            modelBuilder.Entity<Category>().Property(e => e.CategoryName)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<Category>().Property(e => e.CategoryDescription)
                .HasColumnType("nvarchar(255)")
                .HasMaxLength(255);

            modelBuilder.Entity<Category>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();
            //Menu
            modelBuilder.Entity<Menu>().Property(e => e.MenuName)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<Menu>().Property(e => e.MenuDescription)
                .HasColumnType("nvarchar(255)")
                .HasMaxLength(255);

            modelBuilder.Entity<Menu>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //MenuItem
            modelBuilder.Entity<MenuItem>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //Order
            modelBuilder.Entity<Order>().Property(e => e.Status)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Order>().Property(e => e.PaymentMethod)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<Order>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();


            //OrderItem
            modelBuilder.Entity<OrderItem>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //Voucher
            modelBuilder.Entity<Voucher>().Property(e => e.VoucherCode)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Voucher>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //CustomerVoucher
            modelBuilder.Entity<CustomerVoucher>().Property(e => e.Status)
                .HasColumnType("nvarchar(20)")
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<CustomerVoucher>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();

            //Reservation
            modelBuilder.Entity<Reservation>().Property(e => e.CustomerName)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<Reservation>().Property(e => e.CustomerPhone)
                .HasColumnType("varchar(12)")
                .HasMaxLength(12)
                .IsRequired();

            modelBuilder.Entity<Reservation>().Property(e => e.PaymentMethod)
                .HasColumnType("nvarchar(30)")
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<Reservation>().Property(e => e.IsDeleted)
                .HasColumnType("bit")
                .IsRequired();
            #endregion
            #region Custom RelationShip
            modelBuilder.Entity<Account>().HasKey(b => b.AccountId);
            modelBuilder.Entity<Role>().HasKey(l => l.RoleId);
            modelBuilder.Entity<RoleAccount>().HasKey(l => l.RoleaccountId);

            modelBuilder.Entity<Menu>().HasKey(c => c.MenuId);
            modelBuilder.Entity<MenuItem>().HasKey(c => c.MenuItemId);
            modelBuilder.Entity<Product>().HasKey(l => l.ProductId);
            modelBuilder.Entity<Order>().HasKey(d => d.OrderId);
            modelBuilder.Entity<OrderItem>().HasKey(e => e.OrderItemId);

            modelBuilder.Entity<Customer>().HasKey(e => e.CustomerId);
            modelBuilder.Entity<Voucher>().HasKey(l => l.VoucherId);
            modelBuilder.Entity<CustomerVoucher>().HasKey(l => l.CustomerVoucherId);

            modelBuilder.Entity<Category>().HasKey(k => k.CategoryId);
            modelBuilder.Entity<Unit>().HasKey(l => l.UnitId);

            modelBuilder.Entity<Employee>().HasKey(l => l.EmployeeId);
            modelBuilder.Entity<Shifte>().HasKey(l => l.Shifte_Id);
            modelBuilder.Entity<EmployeeShifte>().HasKey(l => l.EmployeeId);

            modelBuilder.Entity<Reservation>().HasKey(l => l.ReservationId);
            modelBuilder.Entity<Table>().HasKey(l => l.TableId);
            modelBuilder.Entity<RewardPointe>().HasKey(l => l.RewardPoint);


            //Account
            modelBuilder.Entity<Account>()
               .HasOne(a => a.Employees)
               .WithOne(e => e.Accounts)
               .HasForeignKey<Employee>(e => e.AccountId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customers)
                .WithOne(c => c.Accounts)
                .HasForeignKey<Customer>(c => c.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasMany(a => a.RoleAccounts)
                .WithOne(ra => ra.Accounts)
                .HasForeignKey(ra => ra.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            //Category
            modelBuilder.Entity<Category>()
                .HasMany(a => a.Products)
                .WithOne(ra => ra.Categories)
                .HasForeignKey(ra => ra.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            //Customer
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Accounts)
                .WithOne(a => a.Customers)
                .HasForeignKey<Customer>(c => c.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.RewardPoints)
                .WithOne(r => r.Customers)
                .HasForeignKey<RewardPointe>(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            //CustomerVoucher
            modelBuilder.Entity<CustomerVoucher>()
                .HasOne(cv => cv.Customers)
                .WithMany(c => c.CustomerVouchers)
                .HasForeignKey(cv => cv.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerVoucher>()
                .HasOne(cv => cv.Vouchers)
                .WithMany(v => v.CustomerVouchers)
                .HasForeignKey(cv => cv.VoucherId)
                .OnDelete(DeleteBehavior.Restrict);

            //Employee
            modelBuilder.Entity<Employee>()
                .HasOne(c => c.Accounts)
                .WithOne(a => a.Employees)
                .HasForeignKey<Employee>(c => c.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            //EmployeeShifte
            modelBuilder.Entity<EmployeeShifte>()
                .HasOne(es => es.Employees)
                .WithMany(e => e.EmployeeShiftes)
                .HasForeignKey(es => es.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<EmployeeShifte>()
                .HasOne(es => es.Shiftes)
                .WithMany(s => s.EmployeeShiftes)
                .HasForeignKey(es => es.ShifteId)
                .OnDelete(DeleteBehavior.Restrict);

            //MenuItem
            modelBuilder.Entity<MenuItem>()
                .HasOne(es => es.Menus)
                .WithMany(s => s.MenuItems)
                .HasForeignKey(es => es.MenuId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MenuItem>()
                .HasOne(es => es.Products)
                .WithMany(s => s.MenuItems)
                .HasForeignKey(es => es.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            //Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Tables)
                .WithMany(t => t.Orders)
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customers)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.CustomerVouchers)
                .WithOne(cv => cv.Order)
                .HasForeignKey<Order>(o => o.CustomerVoucherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
               .HasOne(o => o.Employee)
               .WithMany(rp => rp.Orders)
               .HasForeignKey(rp => rp.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

            //OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(o => o.Orders)
                .WithMany(cv => cv.OrderItems)
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(o => o.Products)
                .WithMany(cv => cv.OrderItems)
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            //Product
            modelBuilder.Entity<Product>()
                .HasOne(o => o.Categories)
                .WithMany(cv => cv.Products)
                .HasForeignKey(o => o.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(o => o.Units)
                .WithMany(cv => cv.Products)
                .HasForeignKey(o => o.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            //Reservation
            modelBuilder.Entity<Reservation>()
                .HasOne(o => o.Tables)
                .WithMany(cv => cv.Reservations)
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            //RewardPoints
            modelBuilder.Entity<RewardPointe>()
                .HasOne(o => o.Customers)
                .WithOne(cv => cv.RewardPoints)
                .HasForeignKey<Customer>(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RewardPointe>()
                .HasOne(rp => rp.Orders)
                .WithOne(o => o.RewardPointes)
                .HasForeignKey<RewardPointe>(rp => rp.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            //RoleAccount
            modelBuilder.Entity<RoleAccount>()
               .HasOne(o => o.Roles)
               .WithMany(cv => cv.RoleAccounts)
               .HasForeignKey(o => o.RoleId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoleAccount>()
                .HasOne(rp => rp.Accounts)
                .WithMany(o => o.RoleAccounts)
                .HasForeignKey(rp => rp.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            //Table
            modelBuilder.Entity<Table>()
               .HasOne(o => o.Floors)
               .WithMany(cv => cv.Tables)
               .HasForeignKey(o => o.FloorId)
               .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
        #region DbSet
        public DbSet<Role> Roles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<CustomerVoucher> CustomerVouchers { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<RoleAccount> RoleAccounts { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Shifte> Shiftes { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeShifte> EmployeeShiftes { get; set; }
        public DbSet<RewardPointe> RewardPointes { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Floor> Floors { get; set; }
        #endregion
    }

}


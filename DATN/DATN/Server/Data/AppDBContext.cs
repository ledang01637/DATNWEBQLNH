using DATN.Shared;
using Microsoft.EntityFrameworkCore;

namespace DATN.Server.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().Property(c => c.TotalRewardPoint).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Employee>().Property(d => d.Salary).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Menu>().Property(f => f.PriceCombo).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>().Property(f => f.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>().Property(f => f.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>().Property(f => f.TotalPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(f => f.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Voucher>().Property(f => f.DiscountValue).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Account>().HasKey(b => b.AccountId);
            modelBuilder.Entity<Menu>().HasKey(c => c.MenuId);
            modelBuilder.Entity<Order>().HasKey(d => d.OrderId);
            modelBuilder.Entity<OrderItem>().HasKey(e => e.OrderItemId);
            modelBuilder.Entity<Category>().HasKey(k => k.CategoryId);
            modelBuilder.Entity<Product>().HasKey(l => l.ProductId);
            modelBuilder.Entity<Role>().HasKey(l => l.RoleId);

        }
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
    }

}


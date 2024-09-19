using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DATN.Server.Data;
using DATN.Server.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System;
using Microsoft.AspNetCore.Authentication.Google;

namespace DATN.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();
            services.AddRazorPages();

            var key = Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
                optionos =>
                {
                    optionos.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                
            })
            .AddCookie();

            services.AddScoped<AccountService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<CustomerService>();
            services.AddScoped<CustomerVoucherService>();
            services.AddScoped<EmployeeService>();
            services.AddScoped<EmployeeShifteService>();
            services.AddScoped<FloorService>();
            services.AddScoped<MenuItemService>();
            services.AddScoped<MenuService>();
            services.AddScoped<OrderItemService>();
            services.AddScoped<OrderService>();
            services.AddScoped<ProductService>();
            services.AddScoped<ReservationService>();
            services.AddScoped<RewardPointeService>();
            services.AddScoped<RoleAccountService>();
            services.AddScoped<RoleService>();
            services.AddScoped<ShifteService>();
            services.AddScoped<TableService>();
            services.AddScoped<UnitService>();
            services.AddScoped<VoucherService>();

            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnect"))
            );
            var key = Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
                optionos =>
                {
                    optionos.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;

            })
            .AddCookie()
            .AddGoogle(googleOptions =>
            {
                IConfigurationSection googleAuthNSection = Configuration.GetSection("Authentication:Google");

                googleOptions.ClientId = googleAuthNSection["ClientId"];
                googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
                googleOptions.CallbackPath = "/signin-google";
            })
            .AddFacebook(facebookOptions =>
            {
                IConfigurationSection facebookAuthNSection = Configuration.GetSection("Authentication:Facebook");
                facebookOptions.AppId = facebookAuthNSection["AppId"];
                facebookOptions.AppSecret = facebookAuthNSection["AppSecret"];
                facebookOptions.CallbackPath = "/signin-facebook";
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using MVC_Tiraz.Services;
using MVC_Tiraz.Utilities;
using MVC_Tiraz.Utilities.DBSeeder;
using Stripe;
using TIRAZ.Data;
using TIRAZ.Data;
using TIRAZ.Repositories;

namespace MVC_Tiraz
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure DbContext
            var connectionstring =
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string" + "'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionstring);
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.AllowedUserNameCharacters =
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            //builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            builder.Services.AddAuthentication()
            .AddGoogle("google", opt =>
            {
                var googleAuth = builder.Configuration.GetSection("Authentication:Google");
                opt.ClientId = googleAuth["ClientId"];
                opt.ClientSecret = googleAuth["ClientSecret"];
                opt.SignInScheme = IdentityConstants.ExternalScheme;
            });

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IDBInitialization, DBInitialization>();
            builder.Services.AddScoped<IImageStorage, LocalImageStorage>();
            builder.Services.AddSingleton<IImageValidator, ImageValidator>();


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbInitilizer = scope.ServiceProvider.GetRequiredService<IDBInitialization>();
                await dbInitilizer.InitializeAsync();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseStatusCodePagesWithReExecute("/Admin/Home/NotFoundPage");

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
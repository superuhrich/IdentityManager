using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityManager.Data;
using IdentityManager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace IdentityManager {
	public class Startup {
		public Startup( IConfiguration configuration ) {
			Configuration = configuration;
		}

		public IConfiguration Configuration {
			get;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices( IServiceCollection services ) {
			services.AddDbContext<ApplicationDbContext>(options=> options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
			services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
			services.AddTransient<IEmailSender, MailJetEmailSender>();
			services.Configure<IdentityOptions>(opt=> {
				opt.Password.RequiredLength = 5;
				opt.Password.RequireLowercase = true;
				opt.Lockout.DefaultLockoutTimeSpan= TimeSpan.FromMinutes(30);
				opt.Lockout.MaxFailedAccessAttempts = 2;
			});
			services.ConfigureApplicationCookie( opt => {
				opt.AccessDeniedPath = new PathString( "/Home/Accessdenied" );
			} );
			services.AddAuthentication().AddFacebook( options => {
				options.AppId     = "741643686538450";
				options.AppSecret = "fe22d55b1d52e0533d694890e7842db6";
			} );
			services.AddControllersWithViews();
			services.AddRazorPages();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure( IApplicationBuilder app, IWebHostEnvironment env ) {
			if ( env.IsDevelopment() ) {
				app.UseDeveloperExceptionPage();
			} else {
				app.UseExceptionHandler( "/Home/Error" );
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseAuthentication();

			app.UseAuthorization();

			app.UseEndpoints( endpoints => {
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}" );
				endpoints.MapRazorPages();
			} );
		}
	}
}

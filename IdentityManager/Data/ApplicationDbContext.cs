using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityManager.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityManager.Data {

	public class ApplicationDbContext : IdentityDbContext {

		public ApplicationDbContext(DbContextOptions options): base(options) {
			
		}
		
		public DbSet<ApplicationUser> ApplicationUser { get; set; }

	}
}

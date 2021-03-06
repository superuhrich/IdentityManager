using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using IdentityManager.Data;


namespace IdentityManager.Controllers {

	


	public class UserController:Controller {

		private readonly ApplicationDbContext _db;
		private readonly UserManager<IdentityUser> _userManager;

		public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)  {
			this._db = db;
			this._userManager = userManager;
		}

		public IActionResult Index() {
			var userList = this._db.ApplicationUser.ToList();
			var userRole = this._db.UserRoles.ToList();
			var roles = this._db.Roles.ToList();

			foreach(var user in userList) {
				var role = userRole.FirstOrDefault(u => u.UserId == user.Id);
				if(role==null) {
					user.Role= "None";
				} else {
					user.Role = roles.FirstOrDefault(u => u.Id == role.RoleId)?.Name;
		
				}

			}
			return View(userList);
		}


	}
}

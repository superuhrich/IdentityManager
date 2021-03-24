using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using IdentityManager.Data;

namespace IdentityManager.Controllers {
	public class RolesController:Controller {
		private readonly ApplicationDbContext      _db;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public RolesController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)  {
			this._db          = db;
			this._userManager = userManager;
			this._roleManager = roleManager;
		}
		public IActionResult Index() {
			var roles = this._db.Roles.ToList();

			return View(roles);
		}

		[HttpGet]
		public IActionResult Upsert(string id) {
			if(string.IsNullOrEmpty(id)) {
				return View();
			} else {
				//update
				var objFromDb = this._db.Roles.FirstOrDefault(u => u.Id == id);
				return View(objFromDb);
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Upsert(IdentityRole roleObj) {
			if(await this._roleManager.RoleExistsAsync(roleObj.Name)) {
				//Error
				TempData[SD.Error] = "Role already exists.";
				return RedirectToAction(nameof(Index));
			}
			if(string.IsNullOrEmpty(roleObj.Id)) {
				//create
				await this._roleManager.CreateAsync(new IdentityRole() {Name = roleObj.Name});
				TempData[SD.Success] = "Role created successfully.";
			}else {
				//update
				var objRoleFromDb = this._db.Roles.FirstOrDefault(u =>u.Id == roleObj.Id);
				if(objRoleFromDb == null) {
					TempData[SD.Error] = "Role not found.";
					return RedirectToAction(nameof(Index));
				}
				objRoleFromDb.Name = roleObj.Name;
				objRoleFromDb.NormalizedName = roleObj.Name.ToUpper();
				var result = await this._roleManager.UpdateAsync(objRoleFromDb);

			}
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id) {
			var objFromDb = this._db.Roles.FirstOrDefault(u => u.Id == id);
			var userRolesForThisRole = this._db.UserRoles.Count( u => u.RoleId == id );
			if(userRolesForThisRole > 0 ) {
				TempData[SD.Error] = "You cannot delete this role, since there are users assigned to this role.";
				return RedirectToAction(nameof(Index)); 
			}

			await this._roleManager.DeleteAsync(objFromDb);
			TempData[SD.Success]= "Role deleted succesfully";
			return RedirectToAction(nameof(Index));
		}
	}
}

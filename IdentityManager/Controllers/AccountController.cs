using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace IdentityManager.Controllers {
	[Authorize]

	public class AccountController : Controller {
		private readonly UserManager<IdentityUser>   _userManager;
		private readonly RoleManager<IdentityRole>   _roleManager;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly IEmailSender                _emailSender;

		public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> roleManager ) {
			this._userManager   = userManager;
			this._signInManager = signInManager;
			this._emailSender   = emailSender;
			this._roleManager   = roleManager;
		}

		//GET
		//public IActionResult Index() {
		//	return View();
		//}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Register(string returnUrl=null) {
			if ( !await this._roleManager.RoleExistsAsync( "Admin" ) ) {
				//Create the roles
				await this._roleManager.CreateAsync( new IdentityRole( "Admin" ) );
				await this._roleManager.CreateAsync( new IdentityRole( "User" ) );
			}

			List<SelectListItem> listItems = new List<SelectListItem>();
			listItems.Add(new SelectListItem() {
				Value= "Admin",
				Text= "Admin"
			}  );
			listItems.Add(new SelectListItem() {
				Value = "User",
				Text  = "User"
			}  );


			
			ViewData["ReturnUrl"] = returnUrl;
			RegisterViewModel registerViewModel = new RegisterViewModel() {
				RoleList = listItems
			};
			return View( registerViewModel );
		}


		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl=null) {
			ViewData["ReturnUrl"] = returnUrl;
			returnUrl = returnUrl ?? Url.Content("~/");
			if(ModelState.IsValid) {
				var user = new ApplicationUser {UserName = model.Email, Email= model.Email, Name = model.Name};
				var result = await this._userManager.CreateAsync(user, model.Password);
				if(result.Succeeded) {
					if ( !string.IsNullOrEmpty( model.RoleSelected ) && model.RoleSelected == "Admin" ) {
						await this._userManager.AddToRoleAsync( user, "Admin" );
					} else {
						await this._userManager.AddToRoleAsync( user, "User" );
					}
					var code        = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
					var callBackUrl = Url.Action( "ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme );

					await this._emailSender.SendEmailAsync( model.Email, "Confirm Email - Identity Manager", "Please confirm your email by clicking here: <a href=\"" + callBackUrl + "\">Confirm</a>" );

					await this._signInManager.SignInAsync(user,isPersistent:false);
					return LocalRedirect(returnUrl);
				}
				AddErrors(result);
			}
			List<SelectListItem> listItems = new List<SelectListItem>();
			listItems.Add(new SelectListItem() {
				Value = "Admin",
				Text  = "Admin"
			}  );
			listItems.Add(new SelectListItem() {
				Value = "User",
				Text  = "User"
			}  );
			model.RoleList = listItems;
			return View( model );
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ConfirmEmail(string userId, string code) {
			if(userId == null || code == null) {
				return View("Error");
			}
			var user = await this._userManager.FindByIdAsync(userId);
			if(user == null) {
				return View("Error");
			}
			var result = await this._userManager.ConfirmEmailAsync(user, code);
			return View(result.Succeeded ? "ConfirmEmail" : "Error");
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Login(string returnUrl=null) {
			ViewData["ReturnUrl"] = returnUrl;
			

			return View();
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl=null) {
			ViewData["ReturnUrl"] = returnUrl;
			returnUrl             = returnUrl??Url.Content("~/");
			if ( ModelState.IsValid ) {
				var result = await this._signInManager.PasswordSignInAsync(model.Email, model.Password,model.RememberMe, lockoutOnFailure:true);
				if (result.Succeeded) {
					return LocalRedirect(returnUrl);
				}if(result.IsLockedOut) {
					return View("Lockout");
				}
				else {
					ModelState.AddModelError(string.Empty, "Invalid login attempt");
					return View(model);
				}
			}
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> LogOff() {
	
			await this._signInManager.SignOutAsync();
			
			return RedirectToAction(nameof(HomeController.Index), "Home");
		}

		[HttpGet]
		public async Task<IActionResult> ForgotPassword() {

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model) {
			if ( ModelState.IsValid ) {
				var user = await this._userManager.FindByEmailAsync( model.Email );
				if ( user == null ) {
					return RedirectToAction( "ForgotPasswordConfirmation", "Account" );
				}

				var code        = await this._userManager.GeneratePasswordResetTokenAsync( user );
				var callBackUrl = Url.Action( "ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme );

				await this._emailSender.SendEmailAsync( model.Email, "Reset Password - Identity Manager", "Please reset your password by clicking here: <a href=\"" + callBackUrl + "\">Reset Password</a>" );

				return RedirectToAction( "ForgotPasswordConfirmation", "Account" );


			}

			return View(model);
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgotPasswordConfirmation() {
			return View();
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult ResetPasswordConfirmation() {
			return View();
		}



		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ResetPassword(string code = null) {

			return code==null? View("Error"): View();
			
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model) {
			if ( ModelState.IsValid ) {
				var user = await this._userManager.FindByEmailAsync( model.Email );
				if ( user == null ) {
					return RedirectToAction( "ResetPasswordConfirmation", "Account" );
				}

				var result = await this._userManager.ResetPasswordAsync(user, model.Code, model.Password);
				if(result.Succeeded) {
					return RedirectToAction("ResetPasswordConfirmation");
				}
				AddErrors(result);

			}

			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public IActionResult ExternalLogin(string provider, string returnUrl = null) {
			//request a redictect to the external login provider
			var redirectUrl = Url.Action( "ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl } );
			var properties  = this._signInManager.ConfigureExternalAuthenticationProperties( provider, redirectUrl );
			return this.Challenge( properties, provider );
		}
		
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null) {
			returnUrl ??= this.Url.Content( "~/" );
			if ( remoteError != null ) {
				this.ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
				return  View( nameof( Login ) );
			}
			var info = await this._signInManager.GetExternalLoginInfoAsync();
			if ( info == null ) {
				return this.RedirectToAction( nameof( Login ) );
			}
			//Sign in the user with this external login provider, if the user already has a login. 
			var result = await this._signInManager.ExternalLoginSignInAsync( info.LoginProvider, info.ProviderKey, isPersistent: false );
			if ( result.Succeeded ) {
				//update any authentication tokens
				await this._signInManager.UpdateExternalAuthenticationTokensAsync( info );
				return LocalRedirect( returnUrl );
			} else {
				// if the user does not have an account, we will ask the user to create an account. 
				ViewData["ReturnUrl"] = returnUrl;
				ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
				var email = info.Principal.FindFirstValue( ClaimTypes.Email );
				var name  = info.Principal.FindFirstValue( ClaimTypes.Name );
				return this.View( "ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email, Name= name } );
			}
		}
		
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null) {
			
			if ( ModelState.IsValid ) {
				//get info about user from external login provider
				var info = await this._signInManager.GetExternalLoginInfoAsync();
				if ( info == null ) {
					return this.View( "Error" );
				}
				var user   = new ApplicationUser { UserName = model.Email, Email = model.Email, Name = model.Name };
				var result = await this._userManager.CreateAsync( user );
				if ( result.Succeeded ) {
					await this._userManager.AddToRoleAsync( user, "User" );
					result = await this._userManager.AddLoginAsync( user, info );
					if ( result.Succeeded ) {
						await this._signInManager.SignInAsync( user, isPersistent: false );
						await this._signInManager.UpdateExternalAuthenticationTokensAsync( info );
						return LocalRedirect( returnUrl );
					}
				}
				AddErrors(result);
			}
			ViewData["ReturnUrl"] = returnUrl;
			return this.View( model );
		}

		private void AddErrors(IdentityResult result) {
			foreach ( var error in result.Errors ) {
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}


	}

}

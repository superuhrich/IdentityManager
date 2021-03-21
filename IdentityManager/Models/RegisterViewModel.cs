using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityManager.Models {

	public class RegisterViewModel {

		[Required]
		[EmailAddress]
		[DisplayAttribute(Name = "Email")]
		
		public string Email { get; set; }
		
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		
		public string Password { get; set; }
		
		[DataType( DataType.Password)]
		[Display(Name                    = "Confirm Password")]
		[Compare("Password", ErrorMessage = "The password and confimration password do not match")]
		
	
		
		public string ConfirmPassword { get; set; }
		
		[Required]
		public string Name { get; set; }
		
		public IEnumerable<SelectListItem> RoleList { get; set; }

		public string RoleSelected { get; set; }
		
		

	}

}

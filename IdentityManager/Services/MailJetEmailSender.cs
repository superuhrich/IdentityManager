using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace IdentityManager.Services {
	public class MailJetEmailSender:IEmailSender {

		private readonly IConfiguration _configuration;
		public MailJetOptions _mailJetOptions;

		public MailJetEmailSender(IConfiguration configuration) {
			this._configuration = configuration;
			
		}
		
		// Mailjet login - helpdesk@innov8group.ca
		// Mailjet pass - helpdesk123!

		public async Task SendEmailAsync( string email, string subject, string htmlMessage ) { 

			this._mailJetOptions = this._configuration.GetSection("MailJet").Get<MailJetOptions>();

			MailjetClient client = new MailjetClient(this._mailJetOptions.ApiKey, this._mailJetOptions.SecretKey) {
				//Version = ApiVersion.V3_1,
			};
			MailjetRequest request = new MailjetRequest {
					Resource = Send.Resource,
				}
				.Property(Send.FromEmail, "helpdesk@innov8group.ca")
				.Property(Send.FromName, "Innov8 Password Reset")
				.Property(Send.Subject, subject)
				.Property(Send.HtmlPart, htmlMessage)
				.Property(Send.Recipients, new JArray {
					new JObject {
						{"Email", email}
					}
				});
			MailjetResponse response = await client.PostAsync(request);
			//if (response.IsSuccessStatusCode) {
			//	Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount()));
			//	Console.WriteLine(response.GetData());
			//} else {
			//	Console.WriteLine(string.Format("StatusCode: {0}\n", response.StatusCode));
			//	Console.WriteLine(string.Format("ErrorInfo: {0}\n",  response.GetErrorInfo()));
			//	Console.WriteLine(response.GetData());
			//	Console.WriteLine(string.Format("ErrorMessage: {0}\n", response.GetErrorMessage()));
			//}
		}
	}
}

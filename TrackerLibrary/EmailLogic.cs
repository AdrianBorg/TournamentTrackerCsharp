using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;

namespace TrackerLibrary
{
	public static class EmailLogic
	{
		public static void SendEmail(List<string> to, string subject, string body)
		{
			SendEmail(to, new List<string>(), subject, body);
		}

		public static void SendEmail(List<string> to, List<string> bcc, string subject, string body)
		{
			string fromAddress = GlobalConfig.AppKeyLookup("senderEmail");
			string displayName = GlobalConfig.AppKeyLookup("senderDisplayName");

			MailAddress fromMailAddress = new MailAddress(fromAddress, displayName);

			MailMessage mail = new MailMessage();
			to.ForEach(address => { mail.To.Add(address); });
			bcc.ForEach(address => { mail.Bcc.Add(address); });
			mail.From = fromMailAddress;
			mail.Subject = subject;
			mail.Body = body;
			mail.IsBodyHtml = true;

			SmtpClient client = new SmtpClient();

			client.Send(mail);
		} 
	}
}

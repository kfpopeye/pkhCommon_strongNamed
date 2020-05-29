///***********************************************************************
///**                                                                   **
///**   This file is no longer compiled.                                **
///**                                                                   **
///***********************************************************************
///
using System;
using System.Collections;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;

namespace pkhCommon
{
    public class Email
    {
        //************ Check to see if this can be deleted - March 3, 2020
        //public static void SendErrorMessageAndLog(string AppVersionName, string AppVersionNumber, Exception error, AssemblyName a, log4net.ILog log)
        //{
        //    var rootAppender = ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository())
        //                                   .Root.Appenders.OfType<log4net.Appender.FileAppender>()
        //                                   .FirstOrDefault();

        //    string filename = rootAppender != null ? rootAppender.File : string.Empty;           
        //}

        public static void SendErrorMessage(string AppVersionName, string AppVersionNumber, Exception error, AssemblyName a)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(error.ToString());

            if (error.Data.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Error Data");
                sb.AppendLine("----------");
                foreach (DictionaryEntry de in error.Data)
                {
                    sb.AppendLine(de.Key.ToString() + " : " + de.Value);
                }
            }
            SendErrorMessage(AppVersionName, AppVersionNumber, sb.ToString(), a);
        }


        public static void SendErrorMessage(string AppVersionName, string AppVersionNumber, string error, AssemblyName a)
        {
            CreateHTMLReport(AppVersionName, AppVersionNumber, error, a);
        }

        //Major : Revit version   Minor : App version   Build : App release version    Revision : not used
        private static void CreateHTMLReport(string AppVersionName, string AppVersionNumber, string error, AssemblyName a)
        {
            System.Text.StringBuilder MsgBuilder = new System.Text.StringBuilder();

            MsgBuilder.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" /><title>Report an Error</title></head><body><div align=\"center\"> <table width=\"976\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" bgcolor=\"#ffffff\">  <tr>    <td width=\"169\"><a href=\"http://www.pkhlineworks.ca\"><img src=\"http://www.pkhlineworks.ca/img/logo.gif\" alt=\"PKH Lineworks logo\" width=\"169\" height=\"95\" border=\"0\" /></a></td>  </tr>  <tr>    <td colspan=\"2\" height=\"24\">&nbsp;</td>  </tr>  <tr>    <td colspan=\"2\" height=\"24\" bgcolor=\"#404040\">&nbsp;</td>    </tr>  <tr>    <td colspan=\"2\"><img src=\"img/spacer.gif\" width=\"1\" height=\"1\" border=\"0\" /></td>    </tr></table><table width=\"976\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">  <tr>    <td colspan=\"2\">Your feedback is important. Any additional information you can give will help make these programs even better. For example, what steps did you take when the crash happened? Is this the first time you have run the command? Has the error happened more than once? Does the error happen everytime you run the command?<br><br></td>  </tr>  <form method=\"post\" action=\"http://www.pkhlineworks.ca/appsenderror.php\"><input type=\"hidden\" id=\"errormsg\" name=\"errormsg\" value=\"");
            MsgBuilder.Append(StringHelper.SafeForXML(error));
#if MULTILICENSE
            MsgBuilder.Append("\" /><input type=\"hidden\" id=\"app_name\" name=\"app_name\" value=\"" + a.Name + " ML");
#else
            MsgBuilder.Append("\" /><input type=\"hidden\" id=\"app_name\" name=\"app_name\" value=\"" + a.Name);
#endif
            MsgBuilder.Append("\" /><input type=\"hidden\" id=\"app_ver\" name=\"app_ver\" value=\"" + a.Version.Major.ToString());
            MsgBuilder.Append("\" /><input type=\"hidden\" id=\"app_subver\" name=\"app_subver\" value=\"" + a.Version.Build.ToString());
            MsgBuilder.Append("\" /><input type=\"hidden\" id=\"revitver\" name=\"revitver\" value=\"" + AppVersionName + AppVersionNumber);
            MsgBuilder.Append("\" /><input type=\"hidden\" id=\"winver\" name=\"winver\" value=\"" + System.Environment.OSVersion.ToString());
            MsgBuilder.Append("\" />  <tr><td width=\"400\" valign=\"top\">Your Name: (Optional) <br /><input type=\"text\" name=\"visitor\" size=\"35\" /><br />Your Email: (Optional)<br /><input type=\"text\" name=\"visitormail\" size=\"35\" /><br /><br />Additional Information: (Optional)<br /><textarea name=\"notes\" rows=\"6\" cols=\"40\"></textarea><br /><br /><input type=\"submit\" value=\"Send\" /><br /></td><td align=\"left\" valign=\"top\"><h4>The following error information will be sent.</h4>");

#if MULTILICENSE
            MsgBuilder.Append(a.Name + " ML " + a.Version.Major.ToString() + " Release " + a.Version.Build.ToString() + "<br />");
#else
            MsgBuilder.Append(a.Name + " " + a.Version.Major.ToString() + " Release " + a.Version.Build.ToString() + "<br />");
#endif

            MsgBuilder.Append("Revit: " + AppVersionName + AppVersionNumber + "<br />");
            MsgBuilder.Append("System: " + System.Environment.OSVersion.ToString() + "<br />");
            MsgBuilder.Append(StringHelper.SafeForXML(error));
            MsgBuilder.Append("</td></tr></form></td></tr><tr><td colspan=\"2\" height=\"24\" bgcolor=\"#404040\">&nbsp;</td></tr><tr><td colspan=\"2\"><img src=\"img/spacer.gif\" width=\"1\" height=\"1\" /></td></tr><tr><td colspan=\"2\" height=\"115\" bgcolor=\"#e0e0e0\" align=\"right\" valign=\"bottom\" style=\"padding:0 24px 0 0\"><p class=\"copyright\">Copyright PKH Lineworks. All rights reserved.</p>      </td></tr><tr><td colspan=\"2\" bgcolor=\"#e0e0e0\">&nbsp;</td></tr></table></div></body></html>");

            string errorfile = Environment.GetEnvironmentVariable("temp") + "\\error.html";
            using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(errorfile))
            {
                outfile.Write(MsgBuilder.ToString());
            }

            Windows.Browser thebrowser = new Windows.Browser();
            thebrowser.ShowHelp(errorfile);
        }

        //cqan probably delete this
        //public static string SendMessage(string sendTo, string sendFrom, string sendSubject, string sendMessage)
        //{
        //    try
        //    {
        //        // validate the email address
        //        bool bTest = ValidateEmailAddress(sendTo);

        //        // if the email address is bad, return message
        //        if (bTest == false)
        //            return "Invalid recipient email address: " + sendTo;

        //        // create the email message 
        //        MailMessage message = new MailMessage(sendFrom, sendTo, sendSubject, sendMessage);

        //        // create smtp client at mail server location
        //        SmtpClient client = new
        //        SmtpClient("Properties.Settings.Default.SMTPAddress");

        //        // add credentials
        //        client.UseDefaultCredentials = true;

        //        // send message
        //        client.Send(message);

        //        return "Message sent to " + sendTo + " at " + DateTime.Now.ToString() + ".";
        //    }

        //    catch (Exception ex)
        //    {
        //        return ex.Message.ToString();
        //    }
        //}

        /// <summary>
        /// Transmit an email message with
        /// attachments
        /// </summary>
        /// <param name="sendTo">Recipient Email Address</param>
        /// <param name="sendFrom">Sender Email Address</param>
        /// <param name="sendSubject">Subject Line Describing Message</param>
        /// <param name="sendMessage">The Email Message Body</param>
        /// <param name="attachments">A string array pointing to the location of each attachment</param>
        /// <returns>Status Message as String</returns>

        public static string SendMessageWithAttachment(string sendTo, string sendFrom, string sendSubject, string sendMessage, ArrayList attachments)
        {
            try
            {
                // validate email address
                bool bTest = ValidateEmailAddress(sendTo);

                if (bTest == false)
                    return "Invalid recipient email address: " + sendTo;

                // Create the basic message
                MailMessage message = new MailMessage(sendFrom, sendTo, sendSubject, sendMessage);

                // The attachments array should point to a file location where
                // the attachment resides - add the attachments to the message

                foreach (string attach in attachments)
                {
                    Attachment attached = new Attachment(attach,
                    MediaTypeNames.Application.Octet);
                    message.Attachments.Add(attached);
                }

                // create smtp client at mail server location
                SmtpClient client = new
                SmtpClient("Properties.Settings.Default.SMTPAddress");

                // Add credentials
                client.UseDefaultCredentials = true;

                // send message
                client.Send(message);

                return "Message sent to " + sendTo + " at " + DateTime.Now.ToString() + ".";
            }

            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// Confirm that an email address is valid in format
        /// </summary>
        /// <param name="emailAddress">Full email address to validate</param>
        /// <returns>True if email address is valid</returns>

        public static bool ValidateEmailAddress(string emailAddress)
        {
            try
            {
                string TextToValidate = emailAddress;
                Regex expression = new Regex(@"\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}");

                // test email address with expression
                if (expression.IsMatch(TextToValidate))
                {
                    // is valid email address
                    return true;
                }
                else
                {
                    // is not valid email address
                    return false;
                }
            }

            catch (Exception)
            {
                throw;
            }
        }
    }
}
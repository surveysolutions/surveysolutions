using System;
using System.Collections.Generic;
using System.Text;
using WB.Core.BoundedContexts.Headquarters.Invitations;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class RawMessageBuilder
    {
        public string From { get; set; }
        public string To { get; set; }
        public string ReplyAddress { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
        public string TextBody { get; set; }
        public List<EmailAttachment> Attachments { get; set; }

        public string Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("From: ").Append(From).AppendLine();
            sb.Append("To: ").Append(To).AppendLine();
            if (!string.IsNullOrEmpty(ReplyAddress))
                sb.Append("Reply-To: ").Append(ReplyAddress).AppendLine();
            sb.Append("Subject: =?utf-8?B?").Append(ToBase64String(Subject)).AppendLine("?=");
            
            sb.AppendLine("Content-Type: multipart/mixed;")
                .AppendLine("    boundary=\"a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a\"")
                .AppendLine()
                .AppendLine("--a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a");
            sb.AppendLine("Content-Type: multipart/alternative;")
                .AppendLine("    boundary=\"sub_a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a\"")
                .AppendLine()
                .AppendLine("--sub_a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a");

            sb.AppendLine("Content-Type: text/plain; charset=utf-8")
                .AppendLine()
                .AppendLine(TextBody)
                .AppendLine();
            
            sb.AppendLine("--sub_a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a")
                .AppendLine("Content-Type: text/html; charset=utf-8")
                .AppendLine()
                .AppendLine(HtmlBody)
                .AppendLine()
                .AppendLine("--a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a--");

            if (Attachments != null)
            {
                foreach (var attachment in Attachments)
                {
                    sb.AppendLine("--a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a")
                        .Append("Content-Type: ").Append(attachment.ContentType)
                        .Append("; name=\"").Append(attachment.Filename).Append("\"").AppendLine()
                        .Append("Content-Description: ").AppendLine(attachment.Filename)
                        .Append("Content-Disposition: attachment;filename=\"").Append(attachment.Filename).AppendLine("\";")
                        //             creation-date="Sat, 05 Aug 2017 19:35:36 GMT";
                        .Append("    creation-date=\"").Append(DateTime.UtcNow.ToString()).AppendLine("\";")
                        .AppendLine("Content-Transfer-Encoding: base64")
                        .AppendLine()
                        .AppendLine(attachment.Base64String)
                        .AppendLine()
                        .AppendLine("--a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a--");
                }
            }

            return sb.ToString();
        }

        private string ToBase64String(string text) => 
            Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    }
    
    /*
From: "Sender Name" <sender@example.com>
To: recipient@example.com
Subject: Customer service contact info
Content-Type: multipart/mixed;
    boundary="a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a"

--a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a
Content-Type: multipart/alternative;
    boundary="sub_a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a"

--sub_a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a
Content-Type: text/plain; charset=iso-8859-1
Content-Transfer-Encoding: quoted-printable

Please see the attached file for a list of customers to contact.

--sub_a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a
Content-Type: text/html; charset=iso-8859-1
Content-Transfer-Encoding: quoted-printable

<html>
<head></head>
<body>
<h1>Hello!</h1>
<p>Please see the attached file for a list of customers to contact.</p>
</body>
</html>

--sub_a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a--

--a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a
Content-Type: text/plain; name="customers.txt"
Content-Description: customers.txt
Content-Disposition: attachment;filename="customers.txt";
    creation-date="Sat, 05 Aug 2017 19:35:36 GMT";
Content-Transfer-Encoding: base64

SUQsRmlyc3ROYW1lLExhc3ROYW1lLENvdW50cnkKMzQ4LEpvaG4sU3RpbGVzLENhbmFkYQo5MjM4
OSxKaWUsTGl1LENoaW5hCjczNCxTaGlybGV5LFJvZHJpZ3VleixVbml0ZWQgU3RhdGVzCjI4OTMs
QW5heWEsSXllbmdhcixJbmRpYQ==

--a3f166a86b56ff6c37755292d690675717ea3cd9de81228ec2b76ed4a15d6d1a--     */
}
using NUnit.Framework;
using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V5.CustomFunctions;

namespace WB.Tests.Unit.SharedKernels.DataCollection.CustomFunctions
{
    [TestFixture]
    internal class CustomFunctionsV5Tests
    {
        #region EMAIL TESTS 
        // Examples https://en.wikipedia.org/wiki/Email_address#Examples

        [Test]
        public void Test_EmailValid()
        {
            var validEmails = new List<String>();
            validEmails.Add("prettyandsimple@example.com");
            validEmails.Add("very.common@example.com");
            validEmails.Add("disposable.style.email.with+symbol@example.com");
            validEmails.Add("other.email-with-dash@example.com");
            validEmails.Add("\"much.more unusual\"@example.com");
            validEmails.Add("\"very.unusual.@.unusual.com\"@example.com");
            validEmails.Add("\"very.(),:;<>[]\\\".VERY.\\\"very@\\\\ \\\"very\\\".unusual\"@strange.example.com");
            //validEmails.Add("admin@mailserver1");  // ??
            validEmails.Add("#!$%&'*+-/=?^_`{}|~@example.org");
            validEmails.Add("\"()<>[]:,;@\\\\\\\"!#$%&'*+-/=?^_`{}| ~.a\"@example.org");
            validEmails.Add("\" \"@example.org");
            validEmails.Add("üñîçøðé@example.com");
            validEmails.Add("üñîçøðé@üñîçøðé.com");
            validEmails.Add("чебурашка@ящик-с-апельсинами.рф");
            validEmails.Add("甲斐@黒川.日本");
            validEmails.Add("我買@屋企.香港");
            validEmails.Add("δοκιμή@παράδειγμα.δοκιμή");
            validEmails.Add("Pelé@example.com");
            validEmails.Add("\"john..doe\"@example.com");  // Quoted content should be permitted
            //validEmails.Add("postmaster@.");             // Microsoft disallows this. Only one reference on StackOverflow that this may be a valid email. 

            
            foreach (var email in validEmails)
            {
                Assert.IsTrue(email.IsValidEmail());
            }
        }

        [Test]
        public void Test_EmailInvalid()
        {
            var invalidEmails = new List<String>();
            invalidEmails.Add("Abc.example.com");
            invalidEmails.Add("A@b@c@example.com");
            invalidEmails.Add("a\"b(c)d,e:f;g<h>i[j\\k]l@example.com");
            invalidEmails.Add("just\"not\"right@example.com");
            invalidEmails.Add("this is\"not\\allowed@example.com");
            invalidEmails.Add("this\\ still\\\"not\\\\allowed@example.com");
            invalidEmails.Add("john..doe@example.com");
            invalidEmails.Add("john.doe@example..com");
            invalidEmails.Add(" prettyandsimple@example.com");
            invalidEmails.Add("prettyandsimple@example.com ");
            invalidEmails.Add("abc");

            foreach (var email in invalidEmails)
            {
                Assert.IsFalse(email.IsValidEmail());
            }
        }

        #endregion
    }
}

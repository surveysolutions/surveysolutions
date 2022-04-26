using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WB.UI.Shared.Web.Captcha
{
    public class HostedCaptchaProvider : ICaptchaProvider, IHostedCaptcha
    {
        private const string InputName = "CaptchaInputText";
        private const string TokenName = "CaptchaToken";

        private readonly IDataProtectionProvider protectionProvider;

        public HostedCaptchaProvider(IDataProtectionProvider protectionProvider)
        {
            this.protectionProvider = protectionProvider;
        }

        public Task<bool> IsCaptchaValid(HttpRequest request)
        {
            try
            {
                var inputText = request.Form[InputName][0];
                var token = request.Form[TokenName][0];

                var protector = protectionProvider.CreateProtector("Captcha code protector");
                var code = protector.Unprotect(token);

                if (code.ToUpper() == inputText.ToUpper())
                {
                    return Task.FromResult(true);
                }
            }
            catch
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(false);
        }

        public HtmlString Render<T>(IHtmlHelper<T> html)
        {
            var imageGenerator = new CaptchaImageGenerator();
            var code = RandomString(6);
            var protector = protectionProvider.CreateProtector("Captcha code protector");
           
            var token = protector.Protect(code);
            var imageBytes = imageGenerator.Generate(code);

            var htmlContent = $@"
<input id='{TokenName}' name='{TokenName}' type='hidden' value='{token}'>
<img src='data:image/jpeg;base64, {Convert.ToBase64String(imageBytes)}' />
<br />
<label for='{InputName}'>{Resources.Captcha.EnterText}</label>
<input autocomplete='off' autocorrect='off' data-val='true' id='{InputName}' name='{InputName}' type='text' value='' />";
            
            return new HtmlString(htmlContent);
        }

        /// https://stackoverflow.com/a/54471736/41483
        public static string RandomString(int length)
        {
            // removed j,0, O,v,I
            const string alphabet = "abcdefghikmnopqrstuwxyzABCDEFGHJKLMNPQRSTUVWXYZ123456789";
            var res = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                int count = (int)Math.Ceiling(Math.Log(alphabet.Length, 2) / 8.0);
                Debug.Assert(count <= sizeof(uint));

                int offset = BitConverter.IsLittleEndian ? 0 : sizeof(uint) - count;
                int max = (int)(Math.Pow(2, count * 8) / alphabet.Length) * alphabet.Length;
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (res.Length < length)
                {
                    rng.GetBytes(uintBuffer, offset, count);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    if (num < max)
                    {
                        res.Append(alphabet[(int)(num % alphabet.Length)]);
                    }
                }
            }

            return res.ToString();
        }
    }
}

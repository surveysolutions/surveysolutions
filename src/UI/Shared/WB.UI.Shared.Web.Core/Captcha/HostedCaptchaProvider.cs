using System;
using System.Diagnostics;
using System.Linq;
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
                var captchaInput = request.Form[InputName];
                if (!captchaInput.Any())
                    return Task.FromResult(false);
                
                var inputText = captchaInput[0];
                var token = request.Form[TokenName][0];
                if (inputText == null || token == null)
                    return Task.FromResult(false);

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

        public HtmlString Render()
        {
            var imageGenerator = new CaptchaImageGenerator();
            var code = RandomString(6);
            var protector = protectionProvider.CreateProtector("Captcha code protector");
           
            var token = protector.Protect(code);
            string htmlContent;

            if (imageGenerator.IsFontFound)
            {
                var imageBytes = imageGenerator.Generate(code);
                ArgumentNullException.ThrowIfNull(imageBytes);

                htmlContent = $@"
<input id='{TokenName}' name='{TokenName}' type='hidden' value='{token}'>
<img src='data:image/jpeg;base64, {Convert.ToBase64String(imageBytes)}' />
<br />
<label for='{InputName}'>{Resources.Captcha.EnterText}</label>
<input autocomplete='off' autocorrect='off' data-val='true' id='{InputName}' name='{InputName}' type='text' value='' class='form-control' />";
            }
            else
            {
                htmlContent = $@"<span style='color:red'>{Resources.Captcha.CaptchaError}</span>";
            }
            
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

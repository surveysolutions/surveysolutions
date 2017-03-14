using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Ninject;
using Owin;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.UI.Headquarters.Utils.Auth
{
    public static class BasicApiAuthenticationExtensions
    {
        public static IAppBuilder UseBasicApiAuthentication(this IAppBuilder app, BasicApiAuthOptions options = null)
        {
            if (app == null)
                throw new ArgumentNullException("app");
            app.Use(typeof(BasicApiAuthenticationMiddleware), app, options ?? new BasicApiAuthOptions());
            app.UseStageMarker(PipelineStage.Authenticate);
            return app;
        }
    }

    public class BasicApiAuthOptions : AuthenticationOptions
    {
        public BasicApiAuthOptions() : base(@"BasicApiAuth")
        {
            this.AuthenticationMode = AuthenticationMode.Passive;
        }
        public IKernel Kernel { get; set; }
    }

    public class BasicApiAuthenticationMiddleware : AuthenticationMiddleware<BasicApiAuthOptions>
    {
        public BasicApiAuthenticationMiddleware(OwinMiddleware next, BasicApiAuthOptions options) : base(next, options)
        {

        }

        protected override AuthenticationHandler<BasicApiAuthOptions> CreateHandler()
        {
            return new BasicApiAuthenticationHandler(Options.Kernel.Get<IIdentityManager>());
        }
    }

    internal class BasicApiAuthenticationHandler : AuthenticationHandler<BasicApiAuthOptions>
    {
        //    private const string HeaderNameCacheControl = "Cache-Control";
        //    private const string HeaderNamePragma = "Pragma";
        //    private const string HeaderNameExpires = "Expires";
        //    private const string HeaderValueNoCache = "no-cache";
        //    private const string HeaderValueMinusOne = "-1";
        //private readonly ILogger _logger;
        //private bool _shouldRenew;
        private readonly IIdentityManager identityManager;

        public BasicApiAuthenticationHandler(IIdentityManager identityManager)
        {
            this.identityManager = identityManager;
        }

        // private IReadSideStatusService readSideStatusService;


        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            await Task.CompletedTask;

            //this.Request.
            RequestCookieCollection cookies = this.Request.Cookies;
            return null;
            //string cookie = cookies[this.Options.CookieName];
            //if (string.IsNullOrWhiteSpace(cookie))
            //    return (AuthenticationTicket)null;
            //AuthenticationTicket ticket = this.Options.TicketDataFormat.Unprotect(cookie);
            //if (ticket == null)
            //{
            //    this._logger.WriteWarning("Unprotect ticket failed");
            //    return (AuthenticationTicket)null;
            //}
            //DateTimeOffset currentUtc = this.Options.SystemClock.UtcNow;
            //DateTimeOffset? issuedUtc = ticket.Properties.IssuedUtc;
            //DateTimeOffset? expiresUtc = ticket.Properties.ExpiresUtc;
            //if (expiresUtc.HasValue && expiresUtc.Value < currentUtc)
            //    return (AuthenticationTicket)null;
            //if (issuedUtc.HasValue && expiresUtc.HasValue && this.Options.SlidingExpiration && expiresUtc.Value.Subtract(currentUtc) < currentUtc.Subtract(issuedUtc.Value))
            //{
            //    this._shouldRenew = true;
            //    this._renewIssuedUtc = currentUtc;
            //    this._renewExpiresUtc = currentUtc.Add(expiresUtc.Value.Subtract(issuedUtc.Value));
            //}
            //CookieValidateIdentityContext context = new CookieValidateIdentityContext(this.Context, ticket, this.Options);
            //await this.Options.Provider.ValidateIdentity(context);
            //return new AuthenticationTicket(context.Identity, context.Properties);
        }

        protected override async Task ApplyResponseGrantAsync()
        {
            await Task.CompletedTask;
            //AuthenticationResponseGrant signin = this.Helper.LookupSignIn(this.Options.AuthenticationType);
            //bool shouldSignin = signin != null;
            //AuthenticationResponseRevoke signout = this.Helper.LookupSignOut(this.Options.AuthenticationType, this.Options.AuthenticationMode);
            //bool shouldSignout = signout != null;
            //if (!shouldSignin && !shouldSignout && !this._shouldRenew)
            //    return;
            //CookieOptions cookieOptions = new CookieOptions()
            //{
            //    Domain = this.Options.CookieDomain,
            //    HttpOnly = this.Options.CookieHttpOnly,
            //    Path = this.Options.CookiePath ?? "/"
            //};
            //cookieOptions.Secure = this.Options.CookieSecure != CookieSecureOption.SameAsRequest ? this.Options.CookieSecure == CookieSecureOption.Always : this.Request.IsSecure;
            //if (shouldSignin)
            //{
            //    CookieResponseSignInContext context = new CookieResponseSignInContext(this.Context, this.Options, this.Options.AuthenticationType, signin.Identity, signin.Properties);
            //    DateTimeOffset utcNow = this.Options.SystemClock.UtcNow;
            //    DateTimeOffset dateTimeOffset = utcNow.Add(this.Options.ExpireTimeSpan);
            //    context.Properties.IssuedUtc = new DateTimeOffset?(utcNow);
            //    context.Properties.ExpiresUtc = new DateTimeOffset?(dateTimeOffset);
            //    this.Options.Provider.ResponseSignIn(context);
            //    if (context.Properties.IsPersistent)
            //        cookieOptions.Expires = new DateTime?(dateTimeOffset.ToUniversalTime().DateTime);
            //    this.Response.Cookies.Append(this.Options.CookieName, this.Options.TicketDataFormat.Protect(new AuthenticationTicket(context.Identity, context.Properties)), cookieOptions);
            //}
            //else if (shouldSignout)
            //    this.Response.Cookies.Delete(this.Options.CookieName, cookieOptions);
            //else if (this._shouldRenew)
            //{
            //    AuthenticationTicket model = await this.AuthenticateAsync();
            //    model.Properties.IssuedUtc = new DateTimeOffset?(this._renewIssuedUtc);
            //    model.Properties.ExpiresUtc = new DateTimeOffset?(this._renewExpiresUtc);
            //    string cookieValue = this.Options.TicketDataFormat.Protect(model);
            //    if (model.Properties.IsPersistent)
            //        cookieOptions.Expires = new DateTime?(this._renewExpiresUtc.ToUniversalTime().DateTime);
            //    this.Response.Cookies.Append(this.Options.CookieName, cookieValue, cookieOptions);
            //}
            //this.Response.Headers.Set("Cache-Control", "no-cache");
            //this.Response.Headers.Set("Pragma", "no-cache");
            //this.Response.Headers.Set("Expires", "-1");
            //bool shouldLoginRedirect = shouldSignin && this.Options.LoginPath.HasValue && this.Request.Path == this.Options.LoginPath;
            //bool shouldLogoutRedirect = shouldSignout && this.Options.LogoutPath.HasValue && this.Request.Path == this.Options.LogoutPath;
            //if (!shouldLoginRedirect && !shouldLogoutRedirect || this.Response.StatusCode != 200)
            //    return;
            //string str = this.Request.Query.Get(this.Options.ReturnUrlParameter);
            //if (string.IsNullOrWhiteSpace(str) || !CookieAuthenticationHandler.IsHostRelative(str))
            //    return;
            //this.Options.Provider.ApplyRedirect(new CookieApplyRedirectContext(this.Context, this.Options, str));
        }

        //private static bool IsHostRelative(string path)
        //{
        //    if (string.IsNullOrEmpty(path))
        //        return false;
        //    if (path.Length == 1)
        //        return (int)path[0] == 47;
        //    if ((int)path[0] == 47 && (int)path[1] != 47)
        //        return (int)path[1] != 92;
        //    return false;
        //}

        protected override Task ApplyResponseChallengeAsync()
        {
            if (this.Response.StatusCode != 401)
                return Task.CompletedTask;



            //if (this.Helper.LookupChallenge(this.Options.AuthenticationType, this.Options.AuthenticationMode) != null)
            //{
            //    string str = this.Request.PathBase + this.Request.Path + this.Request.QueryString;
            //    this.Options.Provider.ApplyRedirect(new CookieApplyRedirectContext(
            //        this.Context, this.Options, this.Request.Scheme + Uri.SchemeDelimiter 
            //        + (object)this.Request.Host + (object)this.Request.PathBase + (object)this.Options.LoginPath +
            //        (object)new QueryString(this.Options.ReturnUrlParameter, str)));
            //}
            return Task.CompletedTask;
        }
    }
}
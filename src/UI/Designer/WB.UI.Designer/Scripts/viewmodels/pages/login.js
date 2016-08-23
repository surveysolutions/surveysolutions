Designer.VM.Login = function (loginApiUrl, homePageUrl, shouldUseRecaptcha, googleRecaptchaSiteKey) {
    Designer.VM.Login.superclass.constructor.apply(this, arguments);

    var self = this;

    self.loginApiUrl = loginApiUrl;
    self.homePageUrl = homePageUrl;

    self.recaptchaClientResponse = '';
    self.googleRecaptchaSiteKey = googleRecaptchaSiteKey;
    self.shouldUseRecaptcha = ko.observable(shouldUseRecaptcha);
    self.userName = ko.observable('');
    self.password = ko.observable('');
    self.staySignedIn = ko.observable(false);

    self.loginStatus = ko.observable();

    self.userIsNotAuthorized = ko.computed(function () {
        return self.loginStatus() === Designer.VM.Login.LoginStatus.InvalidLoginOrPassword ||
            self.loginStatus() === Designer.VM.Login.LoginStatus.IsLockedOut ||
            self.loginStatus() === Designer.VM.Login.LoginStatus.NotApproved;
    });

    self.invalidUserNameOrPassword = ko.computed(function () {
        return self.loginStatus() === Designer.VM.Login.LoginStatus.InvalidLoginOrPassword;
    });

    self.invalidCaptcha = ko.computed(function () {
        return self.shouldUseRecaptcha() && (self.loginStatus() === Designer.VM.Login.LoginStatus.InvalidCaptcha);
    });

    self.login = function () {

        self.loginStatus(undefined);

        if (_.isEmpty(self.userName()) || _.isEmpty(self.password())) {
            self.loginStatus(Designer.VM.Login.LoginStatus.InvalidLoginOrPassword);
            return;
        }

        if (self.shouldUseRecaptcha() && self.recaptchaClientResponse === '') {
            self.loginStatus(Designer.VM.Login.LoginStatus.InvalidCaptcha);
            return;
        }

        var loginRequest = {
            userName: self.userName(),
            password: self.password(),
            staySignedIn: self.staySignedIn()
        };

        if (self.shouldUseRecaptcha()) {
            loginRequest.recaptcha = self.recaptchaClientResponse;
        }

        self.SendRequest(self.loginApiUrl,
            loginRequest,
            function(response) {
                self.loginStatus(response.Status);
                $(".server-response-holder").html(response.ResponseMessage);

                switch (response.Status) {
                case Designer.VM.Login.LoginStatus.IsLockedOut:
                case Designer.VM.Login.LoginStatus.NotApproved:
                case Designer.VM.Login.LoginStatus.InvalidLoginOrPassword:
                case Designer.VM.Login.LoginStatus.InvalidCaptcha:
                    if (self.shouldUseRecaptcha()) {
                        grecaptcha.reset();
                    }
                    break;
                case Designer.VM.Login.LoginStatus.CaptchaRequired:
                    self.shouldUseRecaptcha(true);
                    self.renderCaptcha();
                    break;
                case Designer.VM.Login.LoginStatus.Success:
                    self.navigateIfLoggedIn();
                    break;
                }
            },
            false,
            false,
            function(exception) {
                // by anti forgery validation we can get BadRequest exception, so just try to move to home page
                self.navigateIfLoggedIn();
            });
    }

    self.navigateIfLoggedIn = function() {
        if (!_.isUndefined(self.QueryString) && !_.isUndefined(self.QueryString['ReturnUrl']))
            window.location.href = decodeURIComponent(self.QueryString['ReturnUrl']);
        else
            window.location.href = self.homePageUrl;
    }

    self.renderCaptcha = function () {
        if (!self.shouldUseRecaptcha()) return;

        grecaptcha.render('recaptcha', {
            'sitekey': self.googleRecaptchaSiteKey,
            'callback': self.onRecaptchaVerified
        });
    }

    self.onRecaptchaVerified = function (response) {
        self.recaptchaClientResponse = response;
    };
};

Designer.VM.Login.LoginStatus = {
    Success : 1,
    InvalidLoginOrPassword : 2,
    InvalidCaptcha : 3,
    CaptchaRequired: 4,
    IsLockedOut: 5,
    NotApproved: 6
};

Designer.Framework.Classes.inherit(Designer.VM.Login, Designer.VM.BasePage);
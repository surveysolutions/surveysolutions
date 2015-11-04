Designer.VM.Login = function (loginApiUrl, homePageUrl, shouldUseRecaptcha) {
    Designer.VM.Login.superclass.constructor.apply(this, arguments);

    var self = this;

    self.loginApiUrl = loginApiUrl;
    self.homePageUrl = homePageUrl;

    self.shouldUseRecaptcha = ko.observable(shouldUseRecaptcha);
    self.userName = ko.observable('');
    self.password = ko.observable('');
    self.staySignedIn = ko.observable(false);

    self.loginStatus = ko.observable();

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

        if (self.shouldUseRecaptcha() && _.isUndefined(grecaptcha.getResponse())) {
            self.loginStatus(Designer.VM.Login.LoginStatus.InvalidCaptcha);
            return;
        }

        var loginRequest = {
            userName: self.userName(),
            password: self.password(),
            staySignedIn: self.staySignedIn()
        };

        if (self.shouldUseRecaptcha()) {
            loginRequest.recaptcha = grecaptcha.getResponse();
        }

        self.SendRequest(self.loginApiUrl, loginRequest, function(response) {
            self.loginStatus(response.Status);

            switch (response.Status) {
            case Designer.VM.Login.LoginStatus.CaptchaRequired:
                self.shouldUseRecaptcha(true);
                break;
            case Designer.VM.Login.LoginStatus.Success:
                var returnUrl = self.QueryString['ReturnUrl'];
                if (!_.isUndefined(returnUrl))
                    window.location.href = decodeURIComponent(returnUrl);
                else
                    window.location.href = self.homePageUrl;
                break;
            }
        }, false, false, function (exception) {
            // by anti forgery validation we can get BadRequest exception, so just try to move to home page
            window.location.href = self.homePageUrl;
        });
    }
};

Designer.VM.Login.LoginStatus = {
    Success : 1,
    InvalidLoginOrPassword : 2,
    InvalidCaptcha : 3,
    CaptchaRequired : 4
};

Designer.Framework.Classes.inherit(Designer.VM.Login, Designer.VM.BasePage);
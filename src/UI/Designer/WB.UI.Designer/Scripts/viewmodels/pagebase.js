Designer.VM.BasePage = function () {

    var self = this;

    self.IsPageLoaded = ko.observable(false);
    self.IsAjaxComplete = ko.observable(true);
    self.QueryString = location.queryString;

    self.IsAjaxComplete.subscribe(function(isLoaded) {
        if (isLoaded) {
            $('#umbrella').hide();
        } else {
            setTimeout(function() {
                if (!self.IsAjaxComplete()) {
                    $('#umbrella').show();
                }
            }, 500);
        }
    });

    self.CheckForRequestComplete = function() {
        if (!self.IsAjaxComplete()) {
            self.ShowNotification(input.settings.messages.notifyDialogTitle, input.settings.messages.notifyDialogText);
        }
    };

    self.SendRequest = function (requestUrl, args, onSuccess, skipInProgressCheck, allowGet) {

        if (!skipInProgressCheck && !self.IsAjaxComplete()) {
            self.CheckForRequestComplete();
            return;
        }

        self.IsAjaxComplete(false);
        
        $.ajax({
            url: requestUrl,
            type: allowGet === true ? 'get' : 'post',
            data: args,
            dataType: 'json'}).done(function (data) {
            if (!_.isUndefined(onSuccess)) {
                onSuccess(data);
            }
        }).always(function () {
            self.IsPageLoaded(true);
            self.IsAjaxComplete(true);
        });
    };
    
    self.load = function() {
    };
}

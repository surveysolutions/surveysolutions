Supervisor.VM.HealthCheckHeader = function (updateHealthCheckApiUrl, controlId) {
    var self = this;
    self.holder = $(controlId);

    self.load = function () {
        self.updateHealthCheckStatus();
    };

    self.updateHealthCheckStatus = function () {
        var requestHeaders = {};
        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;
        $.ajax({
            url: updateHealthCheckApiUrl,
            type: 'get',
            headers: requestHeaders,
            dataType: 'text'
        }).success(function (data) {
            self.updateIconByStatus(data);
        }).complete(function () {
            _.delay(self.updateHealthCheckStatus, 3 * 60 * 1000);
        });
    };

    self.updateIconByStatus = function(status) {
        if (status == 1) { // good
            self.holder.css('color', 'green');
        } else if (status == 2) { // warning
            self.holder.css('color', 'yellow');
        } else { // down
            self.holder.css('color', 'red');
        }
    }
}
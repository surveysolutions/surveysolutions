Supervisor.VM.HealthCheckHeader = function (updateHealthCheckApiUrl, controlId) {
    var self = this;
    self.holder = $(controlId);

    self.load = function () {
        setInterval(self.updateHealthCheckStatus, 5000);
    };

    self.updateHealthCheckStatus = function () {
        var requestHeaders = {};
        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;
        $.ajax({
            url: updateHealthCheckApiUrl,
            type: 'get',
            headers: requestHeaders,
            dataType: 'text'
        }).done(function(data) {
            self.updateIconByStatus(data);
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
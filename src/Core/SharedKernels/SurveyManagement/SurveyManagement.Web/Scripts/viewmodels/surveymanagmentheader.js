Supervisor.VM.SurveyManagmentHeader = function (updateIncomingPackagesQueueApiUrl, holderId) {
    var self = this;
    self.holder = $(holderId);
    self.holder.html('-');
    self.load = function () {
        setInterval(self.updateIncomingPackagesQueue, 3000);
    };

    self.updateIncomingPackagesQueue = function () {
        var requestHeaders = {};
        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;
        $.ajax({
            url: updateIncomingPackagesQueueApiUrl,
            type: 'get',
            headers: requestHeaders,
            dataType: 'json'
        }).done(function(data) {
            self.holder.html(data);
        });
    };
}
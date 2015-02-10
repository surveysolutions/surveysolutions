Supervisor.VM.SurveyManagmentHeader = function (updateIncomingPackagesQueueApiUrl, holderId) {
    var self = this;
    self.holder = $(holderId);
    self.load = function () {
        self.updateIncomingPackagesQueue();
    };

    self.updateIncomingPackagesQueue = function () {
        var requestHeaders = {};
        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;
        $.ajax({
            url: updateIncomingPackagesQueueApiUrl,
            type: 'get',
            headers: requestHeaders,
            dataType: 'json'
        }).success(function (data) {
            self.holder.find('.sync-queue-size').text(data);
            if (data > 0) {
                self.holder.fadeIn();
            } else {
                self.holder.fadeOut();
            }
        })
        .complete(function() {
            _.delay(self.updateIncomingPackagesQueue, 3000);
        });
    };
}
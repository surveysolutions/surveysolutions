Supervisor.VM.Preloading = function () { };
Supervisor.VM.Preloading.prototype = {};
Supervisor.VM.Preloading.UserPreloading = function (updateVerificationStatusApiUrl, statesToUpdate) {
    Supervisor.VM.Preloading.UserPreloading.superclass.constructor.apply(this, arguments);

    var self = this;
    self.state = ko.observable(0);
    self.createdUsersCount = ko.observable(0);
    self.recordsCount = ko.observable(0);
    self.verificationProgressInPercents = ko.observable(0);
    self.errorMessage = ko.observable(0);
    self.fileName = ko.observable('');
    self.isDeleted = ko.observable(false);
    self.verificationErrors = ko.observableArray([]);
    self.updateVerificationStatusApiUrl = updateVerificationStatusApiUrl;

    self.load = function () {

        self.SendRequest(self.updateVerificationStatusApiUrl, {}, function (processDetails) {
            ko.mapping.fromJS(processDetails, {}, self);
            if (processDetails == null) {
                self.isDeleted(true);
                return;
            }
            self.state(processDetails.State);
            self.fileName(processDetails.FileName);
            self.verificationProgressInPercents(processDetails.VerificationProgressInPercents);
            self.verificationErrors(processDetails.VerificationErrors);
            self.createdUsersCount(processDetails.CreatedUsersCount);
            self.recordsCount(processDetails.RecordsCount);
            self.errorMessage(processDetails.ErrorMessage);
            
            if ($.inArray(self.state(), statesToUpdate)>=0)
                _.delay(self.load, 3000);

        }, true, false);
    };
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.Preloading.UserPreloading, Supervisor.VM.BasePage);

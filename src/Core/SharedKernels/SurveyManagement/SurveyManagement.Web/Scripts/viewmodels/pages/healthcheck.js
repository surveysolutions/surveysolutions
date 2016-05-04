Supervisor.VM.HealthCheck = function (healthCheckUrl) {
    Supervisor.VM.HealthCheck.superclass.constructor.apply(this, arguments);

    var self = this;

    self.apiUrl = healthCheckUrl;
    self.IsShowRequestIndicator(false);

    self.model = ko.observable({});
    self.status = ko.observable('');
    self.hasWarnings = ko.observable(false);
    self.hasErrors = ko.observable(false);

    self.eventstoreConnectionStatus = ko.observable('');
    self.eventstoreConnectionError = ko.observable('');
    self.numberOfUnhandledPackagesStatus = ko.observable('');
    self.numberOfUnhandledPackages = ko.observable('');
    self.numberOfUnhandledPackagesError = ko.observable('');
    self.numberOfSyncPackagesWithBigSize = ko.observable('');
    self.numberOfSyncPackagesWithBigSizeError = ko.observable('');
    self.deniedFoldersStatus = ko.observable('');
    self.deniedFolders = ko.observableArray([]);
    self.readSideStatus = ko.observable('');
    self.readSideError = ko.observable('');


    self.load = function () {
        self.SendRequest(self.apiUrl, {}, function (data) {
            self.model(data);
            self.status(self.getTextStatus(data.Status));
            self.hasWarnings(data.Status == 2);
            self.hasErrors(data.Status == 3);
            self.eventstoreConnectionStatus(self.getTextStatus(data.EventstoreConnectionStatus.Status));
            self.eventstoreConnectionError(data.EventstoreConnectionStatus.ErrorMessage);
            self.numberOfUnhandledPackagesStatus(self.getTextStatus(data.NumberOfUnhandledPackages.Status));
            var numberOfUnhandledPackagesText = data.NumberOfUnhandledPackages.Status == 3
                ? self.getTextStatus(data.NumberOfUnhandledPackages.Status)
                : data.NumberOfUnhandledPackages.Value;
            self.numberOfUnhandledPackages(numberOfUnhandledPackagesText);
            self.numberOfUnhandledPackagesError(data.NumberOfUnhandledPackages.ErrorMessage);
            self.deniedFoldersStatus(self.getTextStatus(data.FolderPermissionCheckResult.Status));
            self.deniedFolders(data.FolderPermissionCheckResult.DeniedFolders);

            self.readSideStatus(self.getTextStatus(data.ReadSideHealthCheckResult.Status));
            self.readSideError(data.ReadSideHealthCheckResult.ErrorMessage);
        }, true, true);
    };

    self.getTextStatus = function (status) {
        if (status == 1)
            return 'Good';
        if (status == 2)
            return 'Warning';
        if (status == 3)
            return 'Error';

        return 'Unknown status';
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.HealthCheck, Supervisor.VM.BasePage);
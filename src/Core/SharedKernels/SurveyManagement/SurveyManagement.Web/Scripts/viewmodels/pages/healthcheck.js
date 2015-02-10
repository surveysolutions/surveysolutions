Supervisor.VM.HealthCheck = function (healthCheckUrl) {
    Supervisor.VM.HealthCheck.superclass.constructor.apply(this, arguments);

    var self = this;

    self.apiUrl = healthCheckUrl;

    self.model = ko.observable({});
    self.status = ko.observable('');
    self.isGood = ko.observable(true);

    self.databaseConnectionStatus = ko.observable('');
    self.databaseConnectionError = ko.observable('');
    self.eventstoreConnectionStatus = ko.observable('');
    self.eventstoreConnectionError = ko.observable('');
    self.numberOfUnhandledPackagesStatus = ko.observable('');
    self.numberOfUnhandledPackages = ko.observable(0);
    self.numberOfUnhandledPackagesError = ko.observable('');
    self.numberOfSyncPackagesWithBigSizeStatus = ko.observable('');
    self.numberOfSyncPackagesWithBigSize = ko.observable(0);
    self.numberOfSyncPackagesWithBigSizeError = ko.observable('');
    self.denidedFoldersStatus = ko.observable('');
    self.denidedFolders = ko.observableArray([]);

    self.load = function () {
        self.SendRequest(self.apiUrl, {}, function (data) {
            self.model(data);
            self.status(self.getTextStatus(data.Status));
            self.isGood(data.Status == 1);
            self.databaseConnectionStatus(self.getTextStatus(data.DatabaseConnectionStatus.Status));
            self.databaseConnectionError(data.DatabaseConnectionStatus.ErrorMessage);
            self.eventstoreConnectionStatus(self.getTextStatus(data.EventstoreConnectionStatus.Status));
            self.eventstoreConnectionError(data.EventstoreConnectionStatus.ErrorMessage);
            self.numberOfUnhandledPackagesStatus(self.getTextStatus(data.NumberOfUnhandledPackages.Status));
            self.numberOfUnhandledPackages(data.NumberOfUnhandledPackages.Value);
            self.numberOfUnhandledPackagesError(data.NumberOfUnhandledPackages.ErrorMessage);
            self.numberOfSyncPackagesWithBigSizeStatus(self.getTextStatus(data.NumberOfSyncPackagesWithBigSize.Value));
            self.numberOfSyncPackagesWithBigSize(data.NumberOfSyncPackagesWithBigSize.Value);
            self.numberOfSyncPackagesWithBigSizeError(data.NumberOfSyncPackagesWithBigSize.ErrorMessage);
            self.denidedFoldersStatus(self.getTextStatus(data.FolderPermissionCheckResult.Status));
            self.denidedFolders(data.FolderPermissionCheckResult.DenidedFolders);
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
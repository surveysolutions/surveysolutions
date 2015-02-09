Supervisor.VM.HealthCheck = function (healthCheckUrl) {
    Supervisor.VM.HealthCheck.superclass.constructor.apply(this, arguments);

    var self = this;

    self.apiUrl = healthCheckUrl;

    self.model = ko.observable({});
    self.status = ko.observable('');

    self.databaseConnectionStatus = ko.observable('');
    self.eventstoreConnectionStatus = ko.observable('');
    self.numberOfUnhandledPackages = ko.observable(0);
    self.numberOfSyncPackagesWithBigSize = ko.observable(0);
    self.denidedFoldersStatus = ko.observable('');
    self.denidedFolders = ko.observableArray([]);

    self.load = function () {
        self.SendRequest(self.apiUrl, {}, function (data) {
            self.model(data);
            self.status(self.getTextStatus(data.Status));
            self.databaseConnectionStatus(self.getTextStatus(data.DatabaseConnectionStatus.Status));
            self.eventstoreConnectionStatus(self.getTextStatus(data.EventstoreConnectionStatus.Status));
            self.numberOfUnhandledPackages(data.NumberOfUnhandledPackages);
            self.numberOfSyncPackagesWithBigSize(data.NumberOfSyncPackagesWithBigSize);

            self.denidedFoldersStatus(data.FolderPermissionCheckResult.DenidedFolders.length > 0 ? 'Error' : 'Good');
            self.denidedFolders(data.FolderPermissionCheckResult.DenidedFolders);
        }, true, true);
    };

    self.getTextStatus = function (status) {
        if (status == 0)
            return 'Good';
        if (status == 1)
            return 'Warning';
        if (status == 2)
            return 'Error';

        return 'Unknown status';
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.HealthCheck, Supervisor.VM.BasePage);
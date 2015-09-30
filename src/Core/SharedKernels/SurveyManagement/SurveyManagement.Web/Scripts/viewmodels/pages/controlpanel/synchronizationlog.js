Supervisor.VM.ControlPanel.SynchronizationLog = function (synchronizationLogUrl, controlPanelSynclogUrl, responsiblesUrl, devicesUrl) {
    Supervisor.VM.ControlPanel.SynchronizationLog.superclass.constructor.apply(this, arguments);

    var dateFormat = "MM/DD/YYYY";

    var self = this;
    self.Url = new Url(controlPanelSynclogUrl);
    self.IsResponsiblesLoading = ko.observable(false);
    self.IsDevicesLoading = ko.observable(false);

    self.ResponsiblesUrl = responsiblesUrl;
    self.DevicesUrl = devicesUrl;

    self.Responsibles = function (query, sync, pageSize) {
        self.IsResponsiblesLoading(true);
        self.SendRequest(self.ResponsiblesUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function () {
            self.IsResponsiblesLoading(false);
        });
    }

    self.Devices = function (query, sync, pageSize) {
        self.IsDevicesLoading(true);
        self.SendRequest(self.DevicesUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.Devices, response.TotalCountByQuery);
        }, true, true, function () {
            self.IsDevicesLoading(false);
        });
    }

    self.SelectedResponsible = ko.observable();
    self.SelectedDevice = ko.observable();
    self.FromDate = ko.observable();
    self.ToDate = ko.observable();

    self.GetFilterMethod = function () {

        var startDate = _.isUndefined(self.FromDate()) ? "" : moment(self.FromDate()).format(dateFormat);
        var endDate = _.isUndefined(self.ToDate()) ? "" : moment(self.ToDate()).format(dateFormat);

        self.Url.query['interviewer'] = _.isUndefined(self.SelectedResponsible()) ? "" : self.SelectedResponsible().UserName;
        self.Url.query['device'] = self.SelectedDevice() || "";
        self.Url.query['from'] = startDate;
        self.Url.query['to'] = endDate;

        if (Modernizr.history) {
            window.history.pushState({}, "Synclog", self.Url.toString());
        }

        return {
            InterviewerName: _.isUndefined(self.SelectedResponsible()) ? "" : self.SelectedResponsible().UserName,
            DeviceId: self.SelectedDevice() || "",
            FromDateTime: startDate,
            ToDateTime: endDate
        };
    };

    self.load = function () {
        if (self.QueryString['interviewer']) {
            self.SelectedResponsible({ UserName: self.QueryString['interviewer'] });
        }

        self.SelectedDevice(self.QueryString['device']);
        if (self.QueryString['from'])
            self.FromDate(unescape(self.QueryString['from']));
        if (self.QueryString['to'])
            self.ToDate(unescape(self.QueryString['to']));
        
        self.Url.query['interviewer'] = self.QueryString['interviewer'] || "";
        self.Url.query['device'] = self.QueryString['device'] || "";
        self.Url.query['from'] = self.QueryString['from'] || "";
        self.Url.query['to'] = self.QueryString['to'] || "";

        self.SelectedResponsible.subscribe(self.filter);
        self.SelectedDevice.subscribe(self.filter);
        self.FromDate.subscribe(self.filter);
        self.ToDate.subscribe(self.filter);

        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ControlPanel.SynchronizationLog, Supervisor.VM.ListView);
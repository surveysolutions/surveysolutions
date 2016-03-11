Supervisor.VM.ControlPanel.BrokenInterviewPackages = function (brokenInperviewPackagesUrl, controlPanelBrokenInperviewPackagesUrl, exceptionTypesUrl) {
    Supervisor.VM.ControlPanel.BrokenInterviewPackages.superclass.constructor.apply(this, arguments);

    var dateFormat = "MM/DD/YYYY";

    var self = this;
    self.Url = new Url(controlPanelBrokenInperviewPackagesUrl);
    self.IsExceptionTypesLoading = ko.observable(false);

    self.ExceptionTypesUrl = exceptionTypesUrl;

    self.ExceptionTypes = function (query, sync, pageSize) {
        self.IsExceptionTypesLoading(true);
        self.SendRequest(self.ExceptionTypesUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.ExceptionTypes, response.TotalCountByQuery);
        }, true, true, function () {
            self.IsExceptionTypesLoading(false);
        });
    }

    self.SelectedExceptionType = ko.observable();
    self.FromProcessingDate = ko.observable();
    self.ToProcessingDate = ko.observable();

    self.GetFilterMethod = function () {

        var startDate = _.isUndefined(self.FromProcessingDate()) ? "" : moment(self.FromProcessingDate()).format(dateFormat);
        var endDate = _.isUndefined(self.ToProcessingDate()) ? "" : moment(self.ToProcessingDate()).format(dateFormat);

        self.Url.query['exceptiontype'] = self.SelectedExceptionType() || "";
        self.Url.query['from'] = startDate;
        self.Url.query['to'] = endDate;

        if (Modernizr.history) {
            window.history.pushState({}, "BrokenInterviewPackages", self.Url.toString());
        }

        return {
            ExceptionType: self.SelectedExceptionType() || "",
            FromProcessingDateTime: startDate,
            ToProcessingDateTime: endDate
        };
    };

    self.load = function () {
        self.SelectedExceptionType(self.QueryString['exceptiontype']);
        if (self.QueryString['from'])
            self.FromProcessingDate(unescape(self.QueryString['from']));
        if (self.QueryString['to'])
            self.ToProcessingDate(unescape(self.QueryString['to']));
        
        self.Url.query['exceptiontype'] = self.QueryString['exceptiontype'] || "";
        self.Url.query['from'] = self.QueryString['from'] || "";
        self.Url.query['to'] = self.QueryString['to'] || "";

        self.SelectedExceptionType.subscribe(self.filter);
        self.FromProcessingDate.subscribe(self.filter);
        self.ToProcessingDate.subscribe(self.filter);

        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ControlPanel.BrokenInterviewPackages, Supervisor.VM.ListView);
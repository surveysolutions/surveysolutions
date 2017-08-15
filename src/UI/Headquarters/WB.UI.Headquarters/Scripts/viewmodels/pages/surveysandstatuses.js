Supervisor.VM.SurveysAndStatuses = function (listViewUrl, responsiblesUrl, statisticsMessage) {
    Supervisor.VM.SurveysAndStatuses.superclass.constructor.apply(this, arguments);

    var self = this;
    self.Url = new Url(window.location.href);
    self.IsResponsiblesLoading = ko.observable(false);
    self.ResponsiblesUrl = responsiblesUrl;
    self.Responsibles = function (query, sync, pageSize) {
        self.IsResponsiblesLoading(true);
        self.SendRequest(self.ResponsiblesUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function () {
            self.IsResponsiblesLoading(false);
        });
    }
    self.SelectedResponsible = ko.observable();
    self.TotalInterviewCount = ko.observable(0);
    self.TotalResponsibleCount = ko.observable(0);
    self.StatisticsMessage = ko.computed(function () {
        return statisticsMessage.replace('{0}', self.TotalInterviewCount());
    }, this);

    self.GetFilterMethod = function () {
        self.Url.query['responsible'] = _.isUndefined(self.SelectedResponsible()) ? "" : self.SelectedResponsible().UserName;
        if (Modernizr.history) {
            window.history.pushState({}, "responsible", self.Url.toString());
        }

        return { ResponsibleName: self.Url.query['responsible'] };
    };
    self.load = function() {
        if (self.QueryString['responsible']) {
            self.SelectedResponsible({ UserName: self.QueryString['responsible'] });
        }
        self.SelectedResponsible.subscribe(function () { self.reloadDataTable(); });

        self.initDataTable(this.onDataTableDataReceived);
        self.reloadDataTable();
    };

    self.onDataTableDataReceived = function(data) {
        self.TotalInterviewCount(data.totalInterviewCount);
        self.TotalResponsibleCount(data.totalResponsibleCount);

        
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SurveysAndStatuses, Supervisor.VM.ListView);
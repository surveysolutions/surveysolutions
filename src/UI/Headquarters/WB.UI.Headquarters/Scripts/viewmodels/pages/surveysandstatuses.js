Supervisor.VM.SurveysAndStatuses = function (listViewUrl, responsiblesUrl) {
    Supervisor.VM.SurveysAndStatuses.superclass.constructor.apply(this, [listViewUrl, undefined, true]);

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
    self.StatisticsMessage = ko.observable("");

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

        self.setStatisticsMessage();
        self.SelectedResponsible.subscribe(function () {
            self.setStatisticsMessage();
            self.reloadDataTable();
        });

        self.initDataTable(this.onDataTableDataReceived);
        self.reloadDataTable();
    };

    self.setStatisticsMessage = function () {
        var message = !_.isUndefined(self.SelectedResponsible())
            ? self.SelectedResponsible().UserName
            : $("#responsibleSelector").attr("placeholder");

        self.StatisticsMessage(message);
    };

    self.onDataTableDataReceived = function(data) {
        if (data.data.length > 0) {
            var totalRow = data.totalRow;
            totalRow.questionnaireVersion = $totalTitle;
            totalRow.questionnaireTitle = "";
            totalRow.DT_RowClass = totalRowClass;
            data.data.unshift(totalRow);
        }
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.SurveysAndStatuses, Supervisor.VM.ListView);
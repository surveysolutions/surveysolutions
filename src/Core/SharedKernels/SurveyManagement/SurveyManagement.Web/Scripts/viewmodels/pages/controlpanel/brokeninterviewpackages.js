Supervisor.VM.ControlPanel.BrokenInterviewPackages = function (brokenInperviewPackagesUrl, controlPanelBrokenInperviewPackagesUrl, exceptionTypesUrl, responsiblesUrl, questionnairesUrl, reprocessUrl) {
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

    self.Questionnaires = ko.observableArray([]);
    self.SelectedQuestionnaire = ko.observable();
    self.SelectedResponsible = ko.observable();
    self.SelectedExceptionType = ko.observable();
    self.FromProcessingDate = ko.observable();
    self.ToProcessingDate = ko.observable();

    self.formatBytes = function (bytes) {
        if (bytes === 0) return '0 Byte';
        var base = 1024;
        var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
        var degree = Math.min(Math.floor(Math.log(bytes) / Math.log(base)), sizes.length - 1);
        var decimalPlaces = Math.min(Math.max(degree - 1, 0), 2);
        return parseFloat((bytes / Math.pow(base, degree)).toFixed(decimalPlaces)) + ' ' + sizes[degree];
    }

    self.GetFilterMethod = function () {

        var startDate = _.isUndefined(self.FromProcessingDate()) ? "" : moment(self.FromProcessingDate()).format(dateFormat);
        var endDate = _.isUndefined(self.ToProcessingDate()) ? "" : moment(self.ToProcessingDate()).format(dateFormat);

        self.Url.query['questionnaire'] = self.SelectedQuestionnaire() || "";
        self.Url.query['exceptiontype'] = self.SelectedExceptionType() || "";
        self.Url.query['from'] = startDate;
        self.Url.query['to'] = endDate;

        if (Modernizr.history) {
            window.history.pushState({}, "BrokenInterviewPackages", self.Url.toString());
        }

        return {
            ExceptionType: self.SelectedExceptionType() || "",
            FromProcessingDateTime: startDate,
            ToProcessingDateTime: endDate,
            ResponsibleId: _.isUndefined(self.SelectedResponsible()) ? "" : self.SelectedResponsible().UserId,
            QuestionnaireIdentity: self.SelectedQuestionnaire()
        };
    };

    self.load = function () {
        self.SendRequest(questionnairesUrl, {}, function (response) {
            self.Questionnaires(response);

            self.SelectedQuestionnaire(self.QueryString['questionnaire']);
            self.SelectedExceptionType(self.QueryString['exceptiontype']);

            if (self.QueryString['from'])
                self.FromProcessingDate(unescape(self.QueryString['from']));
            if (self.QueryString['to'])
                self.ToProcessingDate(unescape(self.QueryString['to']));

            self.Url.query['exceptiontype'] = self.QueryString['exceptiontype'] || "";
            self.Url.query['from'] = self.QueryString['from'] || "";
            self.Url.query['to'] = self.QueryString['to'] || "";
            self.Url.query['questionnaire'] = self.QueryString['questionnaire'] || "";

            self.SelectedExceptionType.subscribe(self.filter);
            self.FromProcessingDate.subscribe(self.filter);
            self.ToProcessingDate.subscribe(self.filter);
            self.SelectedResponsible.subscribe(self.filter);
            self.SelectedQuestionnaire.subscribe(self.filter);

            self.search();

        }, true, true);
    };

    self.reprocessAll = function () {
        bootbox.dialog({
            message: "Are you sure you want to reprocess ALL broken packages?",
            title: "Confirmation",
            buttons: {
                cancel: {
                    label: "No",
                    className: "btn-primary"
                },
                ok: {
                    label: "Yes",
                    className: "btn-danger",
                    callback: function () {
                        self.SendRequest(reprocessUrl, {}, function () {
                            self.search();
                        }, true);
                    }
                }
            }
        });
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ControlPanel.BrokenInterviewPackages, Supervisor.VM.ListView);
Supervisor.VM.ControlPanel.BrokenInterviewPackages = function (brokenInperviewPackagesUrl, controlPanelBrokenInperviewPackagesUrl, exceptionTypesUrl, responsiblesUrl, questionnairesUrl, reprocessSelectedUrl, markReasonAsKnownUrl, knownReasons) {
    Supervisor.VM.ControlPanel.BrokenInterviewPackages.superclass.constructor.apply(this, arguments);

    var dateFormat = "YYYY-MM-DD";

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
    self.InterviewKey = ko.observableArray([]);
    self.SelectedQuestionnaire = ko.observable();
    self.SelectedResponsible = ko.observable();
    self.SelectedExceptionType = ko.observable();
    self.FromProcessingDate = ko.observable();
    self.ToProcessingDate = ko.observable();
    self.SelectedProcessingDateRange = ko.observable();
    $("#dates-range").daterangepicker({
        locale: {
            format: dateFormat
        },
        maxDate: new Date()
    },
    function (start, end) {
        self.setReportRange(start, end);
        self.FromProcessingDate(start);
        self.ToProcessingDate(end);

        self.search();
    });

    self.setReportRange = function (start, end) {
        if (!start || !end) return;

        var formatedStartDate = start.format(dateFormat);
        var formatedEndDate = end.format(dateFormat);
        self.SelectedProcessingDateRange(formatedStartDate + "/" + formatedEndDate);
        $('#dates-range').data('daterangepicker').setStartDate(moment(start));
        $('#dates-range').data('daterangepicker').setEndDate(moment(end));
    };

    self.formatBytes = function (bytes) {
        if (bytes === 0) return '0 Byte';
        var base = 1024;
        var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
        var degree = Math.min(Math.floor(Math.log(bytes) / Math.log(base)), sizes.length - 1);
        var decimalPlaces = Math.min(Math.max(degree - 1, 0), 2);
        return parseFloat((bytes / Math.pow(base, degree)).toFixed(decimalPlaces)) + ' ' + sizes[degree];
    }

    self.GetFilterMethod = function () {

        var startDate = self.FromProcessingDate() != null ? moment(self.FromProcessingDate()) : null;
        var endDate = moment(self.ToProcessingDate());
        if (startDate != null) {
            self.Url.query['from'] = startDate.format(dateFormat);
        }

        self.Url.query['to'] = endDate.format(dateFormat);

        self.Url.query['questionnaire'] = self.SelectedQuestionnaire() || "";
        self.Url.query['exceptiontype'] = self.SelectedExceptionType() || "";
        self.Url.query['interviewkey'] = self.InterviewKey() || "";

        if (Modernizr.history) {
            window.history.pushState({}, null, self.Url.toString());
        }

        return {
            ExceptionType: self.SelectedExceptionType() || "",
            FromProcessingDateTime: startDate != null ? startDate.format(dateFormat) : null,
            ToProcessingDateTime: endDate.format(dateFormat),
            ResponsibleId: _.isUndefined(self.SelectedResponsible()) ? "" : self.SelectedResponsible().UserId,
            QuestionnaireIdentity: self.SelectedQuestionnaire(),
            InterviewKey: self.InterviewKey()
    };
    };

    self.load = function () {
        self.SendRequest(questionnairesUrl, {}, function (response) {
            self.Questionnaires(response);

            self.SelectedQuestionnaire(self.QueryString['questionnaire']);
            self.SelectedExceptionType(self.QueryString['exceptiontype']);
            self.InterviewKey(self.QueryString['interviewkey']);

            var fromPoint = self.QueryString['from'] || null;
            if (fromPoint != null) {
                self.Url.query['from'] = self.QueryString['from'] || null;
            }

            self.Url.query['to'] = self.QueryString['to'] || moment().format(dateFormat);
            
            var from = fromPoint != null ? unescape(fromPoint) : null;
            var to = unescape(self.Url.query['to']);

            self.FromProcessingDate(from);
            self.ToProcessingDate(to);

            self.Url.query['exceptiontype'] = self.QueryString['exceptiontype'] || "";
            self.Url.query['questionnaire'] = self.QueryString['questionnaire'] || "";
            self.Url.query['interviewkey'] = self.QueryString['interviewkey'] || "";

            self.SelectedExceptionType.subscribe(self.filter);
            self.SelectedResponsible.subscribe(self.filter);
            self.SelectedQuestionnaire.subscribe(self.filter);
            self.InterviewKey.subscribe(self.filter);

            self.search();

        }, true, true);
    };

    self.reprocessSelected = function () {
        bootbox.dialog({
            message: "Are you sure you want to reprocess selected broken packages?",
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
                        var request = {
                            packageIds: _.map(self.SelectedItems(), function(package) { return package.Id(); })
                        };

                        self.SendRequest(reprocessSelectedUrl, request, function () { self.search(); }, true);
                    }
                }
            }
        });
    };

    self.putReasonOnPackages = function () {
        bootbox.prompt({
            title: "Put a reason for selected packages",
            inputType: 'select',
            inputOptions: knownReasons,
            callback:
                function (result) {
                    if (result) {
                        var request = {
                            packageIds: _.map(self.SelectedItems(), function (package) { return package.Id(); }),
                            errorType: result
                        };

                        self.SendRequest(markReasonAsKnownUrl, request, function () { self.search(); }, true, false);
                    }
                }
        });
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.ControlPanel.BrokenInterviewPackages, Supervisor.VM.ListView);

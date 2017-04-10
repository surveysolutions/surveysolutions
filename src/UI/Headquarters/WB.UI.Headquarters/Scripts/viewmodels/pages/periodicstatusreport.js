﻿Supervisor.VM.PeriodicStatusReport = function (listViewUrl) {
    Supervisor.VM.PeriodicStatusReport.superclass.constructor.apply(this, arguments);

    var self = this;

    var dateFormat = "MM/DD/YYYY";

    self.Url = new Url(window.location.href);

    self.SelectedQuestionnaire = ko.observable('');

    self.SelectedType = ko.observable(null);

    self.FromDate = ko.observable(null);

    self.Period = ko.observable(null);

    self.ColumnCount = ko.observable(null);

    self.DateTimeRanges = ko.observableArray([]);

    self.TotalRow = ko.observable(null);

    this.QuestionnaireName = ko.observable();

    this.ReportTypeName = ko.observable();

    self.GetPeriodName = function (period) {
        return moment(period.From()).format(dateFormat);
    };

    self.getTotalAverage = function () {
        return (self.TotalRow() || {}).Average || 0;
    };

    self.getTotalCount = function () {
        return (self.TotalRow() || {}).Total || 0;
    };

    self.FormatSpeedPeriod = function(data) {
        if (data === null)
            return "-";
        return moment.duration(data, "minutes").format("D[d] H[h] mm[m]");
    };

    self.FormatQuantityPeriod = function (data) {
        return data;
    };

    var updateQuestionnaireName = function (value) {
        self.QuestionnaireName($("#questionnaireSelector option[value='" + value + "']").text());
    }

    var updateReportTypeName = function (value) {
        self.ReportTypeName($("#reportTypeSelector option[value='" + value + "']").text());
    }

    self.load = function () {
        var todayMinus7Days = moment().add(-6, 'days').format(dateFormat);

        self.Url.query['questionnaireId'] = self.QueryString['questionnaireId'] || "";
        self.Url.query['questionnaireVersion'] = self.QueryString['questionnaireVersion'] || "";
        self.Url.query['from'] = self.QueryString['from'] || todayMinus7Days;
        self.Url.query['period'] = self.QueryString['period'] || "d";
        self.Url.query['columnCount'] = self.QueryString['columnCount'] || "7";
        self.Url.query['reportType'] = self.QueryString['reportType'] || "";

        self.SelectedQuestionnaire("{\"questionnaireId\": \"" + self.QueryString['questionnaireId'] + "\",\"questionnaireVersion\": \"" + self.QueryString['questionnaireVersion'] + "\"}");
        self.SelectedType(self.Url.query['reportType']);

        updateQuestionnaireName(self.SelectedQuestionnaire());
        updateReportTypeName(self.SelectedType());

        var from = unescape(self.Url.query['from']);
        self.FromDate(from);

        var period = unescape(self.Url.query['period']);
        self.Period(period);

        var columnCount = unescape(self.Url.query['columnCount']);
        self.ColumnCount(columnCount);

        self.initReport();

        self.SelectedQuestionnaire.subscribe(function (value) {
            updateQuestionnaireName(self.SelectedQuestionnaire());
            self.initReport();
        });

        self.SelectedType.subscribe(function (value) {
            self.Url.query['reportType'] = value;

            if (Modernizr.history) {
                window.history.pushState({}, "Charts", self.Url.toString());
            }

            location.reload(true);
        });

        self.FromDate.subscribe(function () {
            self.initReport();
        });

        self.Period.subscribe(function () {
            self.initReport();
        });

        self.ColumnCount.subscribe(function () {
            self.initReport();
        });
    };

    self.GetFilterMethod = function () {
        var selectedQuestionnaire = Supervisor.Framework.Objects.isEmpty(self.SelectedQuestionnaire())
           ? { questionnaireId: '', questionnaireVersion: '' }
           : JSON.parse(self.SelectedQuestionnaire());

        var startDate = moment(self.FromDate());

        self.Url.query['questionnaireId'] = selectedQuestionnaire.questionnaireId;
        self.Url.query['questionnaireVersion'] = selectedQuestionnaire.questionnaireVersion;
        self.Url.query['from'] = startDate.format(dateFormat);
        self.Url.query['period'] = self.Period();
        self.Url.query['columnCount'] = self.ColumnCount();
        self.Url.query['reportType'] = self.SelectedType();

        if (Modernizr.history) {
            window.history.pushState({}, "Charts", self.Url.toString());
        }
        return {
            questionnaireId: selectedQuestionnaire.questionnaireId,
            questionnaireVersion: selectedQuestionnaire.questionnaireVersion,
            from: startDate.format(dateFormat),
            period: self.Period(),
            columnCount: self.ColumnCount(),
            supervisorId: self.Url.query['supervisorId'],
            reportType: self.SelectedType()
        };
    };

    self.initReport = function () {
        self.search();
    };
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.PeriodicStatusReport, Supervisor.VM.ListView);
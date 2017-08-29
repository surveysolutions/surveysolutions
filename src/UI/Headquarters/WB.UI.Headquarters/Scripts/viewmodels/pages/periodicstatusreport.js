Supervisor.VM.PeriodicStatusReport = function (listViewUrl) {
    Supervisor.VM.PeriodicStatusReport.superclass.constructor.apply(this, arguments);

    var self = this;
    var defaultFromDate = moment();
    var dateFormat = "YYYY-MM-DD";

    self.Url = new Url(window.location.href);

    self.SelectedQuestionnaire = ko.observable('');

    self.SelectedType = ko.observable(null);

    self.FromDate = ko.observable(defaultFromDate);

    self.Period = ko.observable(null);

    self.ColumnCount = ko.observable(null);

    self.DateTimeRanges = ko.observableArray([]);

    self.TotalRow = ko.observable(null);

    this.QuestionnaireName = ko.observable();

    this.ReportTypeName = ko.observable();

    self.GetPeriodName = function (period) {
        return moment(period.To()).format(dateFormat);
    };

    self.getTotalAverage = function () {
        var average = (self.TotalRow() || {}).Average || null;
        return (ko.isObservable(average)) ? average() : null;
    };

    self.getTotalCount = function () {
        var total = (self.TotalRow() || {}).Total || null;
        return (ko.isObservable(total)) ? total() : null;
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
        var todayMinus7Days = defaultFromDate.format(dateFormat);

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

        self.Period.subscribe(function (newVal) {
            if (newVal === "d") {
                self.ColumnCount(7);
            } else if (newVal === "w") {
                self.ColumnCount(4);
            } else if (newVal === "m") {
                self.ColumnCount(3);
            }
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
            reportType: self.SelectedType(),
            timezoneOffsetMinutes: new Date().getTimezoneOffset()
        };
    };

    self.notifier = new Notifier();

    self.initReport = _.throttle(function () {
        self.notifier.showLoadingIndicator();
        var onSuccess = function() {};
        var onDone = function() {
            self.notifier.hideLoadingIndicator();
        };
        self.search(onSuccess, onDone);
    }, 500, { leading: false });
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.PeriodicStatusReport, Supervisor.VM.ListView);
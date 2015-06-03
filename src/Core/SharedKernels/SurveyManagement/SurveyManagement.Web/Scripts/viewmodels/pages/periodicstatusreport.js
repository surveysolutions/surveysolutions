Supervisor.VM.PeriodicStatusReport = function (listViewUrl) {
    Supervisor.VM.PeriodicStatusReport.superclass.constructor.apply(this, arguments);

    var self = this;
    var dateFormat = "MM/DD/YYYY";

    self.Url = new Url(window.location.href);

    self.SelectedQuestionnaire = ko.observable('');

    self.FromDate = ko.observable(null);

    self.Period = ko.observable(null);

    self.ColumnCount = ko.observable(null);

    self.DateTimeRanges = ko.observableArray([]);

    this.QuestionnaireName = ko.observable();

    self.GetPeriodName = function (period) {
        return ko.computed({
            read: function () {
                return moment(period.To()).format(dateFormat) + "-" + moment(period.From()).format(dateFormat);
            }
        }, this);
    };

    self.FormatSpeedPeriod = function(data) {
        if (data === null)
            return "-";
        return moment.duration(data, "minutes").format("D[d] H[h] mm[m]");
    };

    self.FormatQuantityPeriod = function (data) {
        return data;
    };
    self.load = function () {
        var today = moment().format(dateFormat);

        self.SelectedQuestionnaire("{\"questionnaireId\": \"" + self.QueryString['questionnaireId'] + "\",\"questionnaireVersion\": \"" + self.QueryString['questionnaireVersion'] + "\"}");

        self.Url.query['questionnaireId'] = self.QueryString['questionnaireId'] || "";
        self.Url.query['questionnaireVersion'] = self.QueryString['questionnaireVersion'] || "";
        self.Url.query['from'] = self.QueryString['from'] || today;
        self.Url.query['period'] = self.QueryString['period'] || "d";
        self.Url.query['columnCount'] = self.QueryString['columnCount'] || "7";

        updateQuestionnaireName(self.SelectedQuestionnaire());

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

        if (Modernizr.history) {
            window.history.pushState({}, "Charts", self.Url.toString());
        }

        return {
            questionnaireId: selectedQuestionnaire.questionnaireId,
            questionnaireVersion: selectedQuestionnaire.questionnaireVersion,
            from: startDate.format(dateFormat),
            period: self.Period(),
            columnCount: self.ColumnCount(),
            supervisorId: self.Url.query['supervisorId']
        };
    };

    self.initReport = function () {
        self.search();
    };
    var updateQuestionnaireName = function (value) {
        self.QuestionnaireName($("#questionnaireSelector option[value='" + value + "']").text());
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.PeriodicStatusReport, Supervisor.VM.ListView);
Supervisor.VM.TeamsAndStatuses = function (listViewUrl, $interviewsUrl) {
    Supervisor.VM.TeamsAndStatuses.superclass.constructor.apply(this, arguments);
    
    var self = this;
    self.Url = new Url(window.location.href);
    self.SelectedTemplate = ko.observable('');
    self.QuestionnaireName = ko.observable();
    self.TotalRow = ko.observable({});

    self.GetFilterMethod = function () {
        var selectedTemplate = Supervisor.Framework.Objects.isEmpty(self.SelectedTemplate())
             ? { templateId: '', version: '' }
             : JSON.parse(self.SelectedTemplate());
        self.Url.query['templateId'] = selectedTemplate.templateId;
        self.Url.query['templateVersion'] = selectedTemplate.version;
        if (Modernizr.history) {
            window.history.pushState({}, "Summary", self.Url.toString());
        }

        return {
            TemplateId: selectedTemplate.templateId,
            TemplateVersion: selectedTemplate.version
        };
    };

    var updateQuestionnaireName = function (value) {
        self.QuestionnaireName($("#questionnaireSelector option[value='" + value + "']").text());
    }

    self.footerCallback = function () {
        var api = this.api();
        var totalRow = self.TotalRow();
        $(api.column(1).footer()).text(totalRow.supervisorAssignedCount);
        $(api.column(2).footer()).text(totalRow.interviewerAssignedCount);
        $(api.column(3).footer()).text(totalRow.completedCount);
        $(api.column(4).footer()).text(totalRow.rejectedBySupervisorCount);
        $(api.column(5).footer()).text(totalRow.approvedBySupervisorCount);
        $(api.column(6).footer()).text(totalRow.rejectedByHeadquartersCount);
        $(api.column(7).footer()).text(totalRow.approvedByHeadquartersCount);
        $(api.column(8).footer()).text(totalRow.totalCount);
    };

    self.onDataTableDataReceived = function (data) {
        self.TotalRow(data.totalRow);
    };

    self.load = function () {
        self.SelectedTemplate("{\"templateId\": \"" + self.QueryString['templateId'] + "\",\"version\": \"" + self.QueryString['templateVersion'] + "\"}");

        updateQuestionnaireName(self.SelectedTemplate());

        self.SelectedTemplate.subscribe(function () {
            updateQuestionnaireName(self.SelectedTemplate());
            self.reloadDataTable();
        });

        self.initDataTable(this.onDataTableDataReceived);
        self.reloadDataTable();
    };

    self.getLinkToInterviews = function(data, row, interviewStatus) {
        if (data === 0) return "<span>" + data + "</span>";

        var link = "<a href=\"" + $interviewsUrl + '?templateId=' + row.questionnaireId +
            '&templateVersion=' + row.questionnaireVersion;

        
        link += '&responsible=' + row.responsible;

        if (!_.isUndefined(interviewStatus))
            link += '&status=' + interviewStatus;

        return link + '"\">' + data + "</a>";
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.TeamsAndStatuses, Supervisor.VM.ListView);
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
        if (api.column(0).data().length === 0) {
            $(this).find("tfoot").hide();
        } else {
            $(this).find("tfoot").show();
            $(api.column(1).footer()).text(totalRow.supervisorAssignedCount);
            $(api.column(2).footer()).text(totalRow.interviewerAssignedCount);
            $(api.column(3).footer()).text(totalRow.completedCount);
            $(api.column(4).footer()).text(totalRow.rejectedBySupervisorCount);
            $(api.column(5).footer()).text(totalRow.approvedBySupervisorCount);
            $(api.column(6).footer()).text(totalRow.rejectedByHeadquartersCount);
            $(api.column(7).footer()).text(totalRow.approvedByHeadquartersCount);
            $(api.column(8).footer()).text(totalRow.totalCount);
        }
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

        var queryObject = {};

        if (row.questionnaireId) {
            queryObject.templateId = row.questionnaireId;
            queryObject.templateVersion = row.questionnaireVersion;
        }

        if (row.responsible) {
            queryObject.responsible = row.responsible;
        }

        if (!_.isUndefined(interviewStatus)) {
            queryObject.status = interviewStatus;
        }

        var queryString = $.param(queryObject);
        var linkUrl = $interviewsUrl + (queryString ? "?" + queryString : "");

        return "<a href=\"" + linkUrl + "\">" + data + "</a>";
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.TeamsAndStatuses, Supervisor.VM.ListView);
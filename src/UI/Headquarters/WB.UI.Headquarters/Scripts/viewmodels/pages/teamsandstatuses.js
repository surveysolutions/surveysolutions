﻿Supervisor.VM.TeamsAndStatuses = function (listViewUrl, $interviewsUrl) {
    Supervisor.VM.TeamsAndStatuses.superclass.constructor.apply(this, arguments);
    
    var self = this;
    self.Url = new Url(window.location.href);
    self.SelectedTemplate = ko.observable('');
    self.QuestionnaireName = ko.observable();
    var totalRowClass = 'total-row';

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

    self.onDataTableDataReceived = function (data) {
        if (data.data.length > 0) {
            var totalRow = data.totalRow;
            totalRow.responsible = $totalTitle;
            totalRow.DT_RowClass = totalRowClass;
            data.data.unshift(totalRow);
        }
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
        if (data === 0 || row.DT_RowClass === totalRowClass) return "<span>" + data + "</span>";

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
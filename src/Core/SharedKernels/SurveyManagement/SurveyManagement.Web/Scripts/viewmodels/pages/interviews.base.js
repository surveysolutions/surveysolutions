﻿Supervisor.VM.InterviewsBase = function (serviceUrl, interviewDetailsUrl, users, commandExecutionUrl) {
    Supervisor.VM.InterviewsBase.superclass.constructor.apply(this, [serviceUrl, commandExecutionUrl]);
    
    var self = this;
    
    self.Url = new Url(interviewDetailsUrl);
    self.Users = users;

    self.Templates = ko.observableArray([]);
    self.Responsibles = ko.observableArray([]);
    self.Statuses = ko.observableArray([]);

    self.SelectedTemplate = ko.observable('');
    self.SelectedResponsible = ko.observable('');
    self.SelectedStatus = ko.observable('');
    self.SearchBy = ko.observable('');

    self.getFormattedPrefilledQuestions = function(prefilledQuestions) {
        prefilledQuestions.forEach(function(prefilledQuestion) {
            if (prefilledQuestion.Type() == /*DateTime*/5) {
                prefilledQuestion.Answer(moment(prefilledQuestion.Answer()).format('M/D/YYYY'));
            }
        });

        return prefilledQuestions;
    };
    
    self.GetFilterMethod = function () {

        var selectedTemplate = Supervisor.Framework.Objects.isEmpty(self.SelectedTemplate())
            ? { templateId: '', version: '' }
            : JSON.parse(self.SelectedTemplate());

        self.Url.query['templateId'] = selectedTemplate.templateId;
        self.Url.query['templateVersion'] = selectedTemplate.version;
        self.Url.query['status'] = self.SelectedStatus() || "";
        self.Url.query['interviewerId'] = self.SelectedResponsible() || "";
        self.Url.query['searchBy'] = self.SearchBy() || "";
        
        if (Modernizr.history) {
            window.history.pushState({}, "Interviews", self.Url.toString());
        }

        return {
            TemplateId: selectedTemplate.templateId,
            TemplateVersion: selectedTemplate.version,
            ResponsibleId: self.SelectedResponsible,
            Status: self.SelectedStatus,
            SearchBy: self.SearchBy
        };
    };

    self.load = function () {
        self.SelectedTemplate("{\"templateId\": \"" + self.Url.query['templateId'] + "\",\"version\": \"" + self.Url.query['templateVersion'] + "\"}");
        self.SelectedStatus(self.Url.query['status']);
        self.SelectedResponsible(self.Url.query['interviewerId']);
        self.SearchBy(decodeURIComponent(self.Url.query['searchBy'] || ""));

        self.Url.query['templateId'] = self.Url.query['templateId'] || "";
        self.Url.query['templateVersion'] = self.Url.query['templateVersion'] || "";
        self.Url.query['status'] = self.Url.query['status'] || "";
        self.Url.query['interviewerId'] = self.Url.query['interviewerId'] || "";
        self.Url.query['searchBy'] = self.Url.query['searchBy'] || "";

        self.SelectedTemplate.subscribe(self.filter);
        self.SelectedResponsible.subscribe(self.filter);
        self.SelectedStatus.subscribe(self.filter);

        self.search();
    };

    self.sendCommandAfterFilterAndConfirm = function (commandName, parametersFunc, filterFunc, messageTemplateId, continueMessageTemplateId) {
        var filteredItems = self.GetSelectedItemsAfterFilter(filterFunc);
        var messageHtml = self.getBindedHtmlTemplate(messageTemplateId, filteredItems);

        if (filteredItems.length === 0) {
            bootbox.alert(messageHtml);
            return;
        }

        messageHtml += $(continueMessageTemplateId).html();

        bootbox.confirm(messageHtml, function (result) {
            if (result)
                self.sendCommand(commandName, parametersFunc, filteredItems);
        });
    };

    self.sendCommand = function (commandName, parametersFunc, items) {
        var commands = ko.utils.arrayMap(items, function (rawItem) {
            var item = ko.mapping.toJS(rawItem);
            return ko.toJSON(parametersFunc(item));
        });

        var command = {
            type: commandName,
            commands: commands
        };

        self.SendCommands(command, function () {
            self.load();
        }, true);
    };

    self.getBindedHtmlTemplate = function (templateId, bindObject) {
        var messageTemplate = $("<div/>").html($(templateId).html())[0];
        ko.applyBindings(bindObject, messageTemplate);
        var html = $(messageTemplate).html();
        return html;
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.InterviewsBase, Supervisor.VM.ListView);

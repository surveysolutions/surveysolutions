Supervisor.VM.InterviewsBase = function (serviceUrl, interviewDetailsUrl, responsiblesUrl, users, commandExecutionUrl) {
    Supervisor.VM.InterviewsBase.superclass.constructor.apply(this, [serviceUrl, commandExecutionUrl]);
    
    var self = this;
    
    self.Url = new Url(interviewDetailsUrl);
    self.IsResponsiblesLoading = ko.observable(false);
    self.ResponsiblesUrl = responsiblesUrl;

    self.Responsibles = function (query, sync, pageSize) {
        self.IsResponsiblesLoading(true);
        self.SendRequest(self.ResponsiblesUrl, { query: query, pageSize: pageSize }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function() {
            self.IsResponsiblesLoading(false);
        });
    }

    self.SelectedTemplate = ko.observable('');

    self.SelectedResponsible = ko.observable();

    self.SelectedStatus = ko.observable('');
    self.SearchBy = ko.observable('');

    self.TemplateName = ko.observable();

    self.getFormattedPrefilledQuestions = function(prefilledQuestions) {
        prefilledQuestions.forEach(function(prefilledQuestion) {
            var questionType = prefilledQuestion.Type();
            if (questionType == /*DateTime*/5) {
                prefilledQuestion.Answer(moment(prefilledQuestion.Answer()).format('M/D/YYYY'));
            } else if (questionType == /*Numeric*/4) {
                if (prefilledQuestion.Settings().UseFormating()) {
                    prefilledQuestion.Answer(ko.bindingHandlers.numericformatter.format(prefilledQuestion.Answer()));
                }
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
        self.Url.query['responsible'] = _.isUndefined(self.SelectedResponsible()) ? "" : self.SelectedResponsible().UserName;
        self.Url.query['searchBy'] = self.SearchBy() || "";
        
        if (Modernizr.history) {
            window.history.pushState({}, "Interviews", self.Url.toString());
        }

        return {
            TemplateId: selectedTemplate.templateId,
            TemplateVersion: selectedTemplate.version,
            ResponsibleName: _.isUndefined(self.SelectedResponsible()) ? "" : self.SelectedResponsible().UserName,
            Status: self.SelectedStatus,
            SearchBy: self.SearchBy
        };
    };

    self.load = function () {

        self.SelectedTemplate("{\"templateId\": \"" + self.QueryString['templateId'] + "\",\"version\": \"" + self.QueryString['templateVersion'] + "\"}");
        self.SelectedStatus(self.QueryString['status']);

        if (self.QueryString['responsible']) {
            self.SelectedResponsible({ UserName: self.QueryString['responsible'] });
        }

        self.SearchBy(decodeURIComponent(self.QueryString['searchBy'] || ""));

        updateTemplateName(self.SelectedTemplate());

        self.Url.query['templateId'] = self.QueryString['templateId'] || "";
        self.Url.query['templateVersion'] = self.QueryString['templateVersion'] || "";
        self.Url.query['status'] = self.QueryString['status'] || "";
        self.Url.query['responsible'] = self.QueryString['responsible'] || "";
        self.Url.query['searchBy'] = self.QueryString['searchBy'] || "";

        self.SelectedTemplate.subscribe(
            function (value) {
                updateTemplateName(value);
                self.filter();
            });

        self.SelectedResponsible.subscribe(self.filter);
        self.SelectedStatus.subscribe(self.filter);

        self.search();
    };

    self.sendCommandAfterFilterAndConfirm = function (commandName,
        parametersFunc,
        filterFunc,
        messageTemplateId,
        continueMessageTemplateId,
        onSuccessCommandExecuting,
        onCancelConfirmation,
        modelUpdaterFunc)
    {
            var filteredItems = self.GetSelectedItemsAfterFilter(filterFunc);
            var model = { items: filteredItems }
            if (!_.isUndefined(modelUpdaterFunc)) {
                modelUpdaterFunc(model);
            }

            var messageHtml = self.getBindedHtmlTemplate(messageTemplateId, model);

            if (filteredItems.length === 0) {
                bootbox.alert(messageHtml);
                return;
            }

            messageHtml += $(continueMessageTemplateId).html();

            bootbox.confirm(messageHtml, function (result) {
                if (result) {
                    self.sendCommand(commandName, parametersFunc, filteredItems, onSuccessCommandExecuting);
                } else {
                    if (!_.isUndefined(onCancelConfirmation)) {
                        onCancelConfirmation();
                    }
                }
            });
    };

    self.sendCommand = function (commandName, parametersFunc, items, onSuccessCommandExecuting) {
        var commands = ko.utils.arrayMap(items, function (rawItem) {
            var item = ko.mapping.toJS(rawItem);
            return ko.toJSON(parametersFunc(item));
        });

        var command = {
            type: commandName,
            commands: commands
        };

        self.SendCommands(command, function () {
            if (!_.isUndefined(onSuccessCommandExecuting))
                onSuccessCommandExecuting();
            self.search();
        }, true);
    };

    var updateTemplateName = function (value) {
        self.TemplateName($("#templateSelector option[value='" + value + "']").text());
    }
};
Supervisor.Framework.Classes.inherit(Supervisor.VM.InterviewsBase, Supervisor.VM.ListView);

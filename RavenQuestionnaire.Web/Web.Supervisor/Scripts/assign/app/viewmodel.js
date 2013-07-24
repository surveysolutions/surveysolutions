define('app/viewmodel', ['knockout', 'app/datacontext', 'input'],
    function (ko, datacontext, input) {
        var questionnaire = ko.observable(),
            responsible = ko.observable(),
            questions = ko.observableArray(),
            supervisors = ko.observableArray(),
            errors = ko.observableArray(),
            isSaving = ko.observable(false),
            isSaveEnable = ko.computed(function () {
                var answersAreInvalid = _.any(questions(), function (question) {
                    return question.errors().length > 0;
                });
                return (isSaving() === false) && !answersAreInvalid;
            }),
            save = function () {
                isSaving(true);
                datacontext.save(ko.toJS(responsible), {
                    success: function (response) {
                        if (response.status == "error") {
                            errors.removeAll();
                            errors.push({
                                 error: response.error
                            });
                            $('body').addClass('output-visible');
                        }
                        if (response.status == "ok") {
                            input.backUrl = input.backUrl.replace("_______", datacontext.questionnaire.templateId);
                            window.location = input.backUrl;
                        }
                        isSaving(false);
                    }
                });
            },
            saveCommand = function () {
                isSaving(true);
                datacontext.sendCommand("CreateInterviewWithFeaturedQuestionsCommand", ko.toJS(responsible), {
                    success: function (response) {
                        var backUrl = input.backUrl.replace("_______", datacontext.questionnaire.templateId);
                        window.location = backUrl;
                        isSaving(false);
                    },
                    error: function (response) {
                        errors.removeAll();
                        errors.push({
                            error: response.error
                        });
                        $('body').addClass('output-visible');
                        isSaving(false);
                    }
                });
            },
            hideOutput = function() {
                $('body').removeClass('output-visible');
            },
            init = function () {
                questionnaire(datacontext.questionnaire);
                questions(datacontext.questions.getAllLocal());
                supervisors(datacontext.supervisors.getAllLocal());
            };

        return {
            init: init,
            questions: questions,
            supervisors: supervisors,
            responsible: responsible,
            questionnaire: questionnaire,
            save: save,
            saveCommand : saveCommand,
            hideOutput: hideOutput,
            errors: errors,
            isSaveEnable: isSaveEnable
        };
    });
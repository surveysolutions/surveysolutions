define('app/viewmodel', ['knockout', 'app/datacontext'],
    function (ko, datacontext) {
        var questionnaire = ko.observable(),
            responsible = ko.observable(),
            questions = ko.observableArray(),
            supervisors = ko.observableArray(),
            errors = ko.observableArray();
            save = function () {
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
                            window.back();
                        }
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
            hideOutput: hideOutput,
            errors: errors
        };
    });
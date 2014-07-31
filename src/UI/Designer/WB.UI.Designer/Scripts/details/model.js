define('model',
    [
        'model.questionnaire',
        'model.question',
        'model.staticText',
        'model.group',
        'model.answerOption',
        'model.statistic',
        'model.error'
    ],
function (questionnaire, question, staticText, group, answerOption, statistic, error) {
        var
            model = {
                Questionnaire : questionnaire,
                Question: question,
                StaticText: staticText,
                Group: group,
                AnswerOption: answerOption,
                Statistic: statistic,
                Error: error
            };

        model.setDataContext = function (dc) {
            model.Questionnaire.datacontext(dc);
            model.Group.datacontext(dc);
            model.Question.datacontext(dc);
            model.Statistic.datacontext(dc);
            model.Error.datacontext(dc);
        };

        return model;
    });
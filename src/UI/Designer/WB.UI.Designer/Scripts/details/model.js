define('model',
    [
        'model.questionnaire',
        'model.question',
        'model.group',
        'model.answerOption',
        'model.statistic'
    ],
function (questionnaire, question, group, answerOption, statistic) {
        var
            model = {
                Questionnaire : questionnaire,
                Question: question,
                Group: group,
                AnswerOption: answerOption,
                Statistic: statistic
            };

        model.setDataContext = function (dc) {
            model.Questionnaire.datacontext(dc);
            model.Group.datacontext(dc);
            model.Question.datacontext(dc);
            model.Statistic.datacontext(dc);
        };

        return model;
    });
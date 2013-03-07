define('model',
    [
        'model.questionnaire',
        'model.question',
        'model.group',
        'model.answerOption'
    ],
function (questionnaire, question, group, answerOption) {
        var
            model = {
                Questionnaire : questionnaire,
                Question: question,
                Group: group,
                AnswerOption: answerOption
            };

        model.setDataContext = function (dc) {
            model.Questionnaire.datacontext(dc);
            model.Group.datacontext(dc);
            model.Question.datacontext(dc);
        };

        return model;
    });
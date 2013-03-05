define('model',
    [
        'model.questionnaire',
        'model.question',
        'model.group',
        'model.menuItem',
        'model.answerOption'
    ],
function (questionnaire, question, group, menuItem, answerOption) {
        var
            model = {
                Questionnaire : questionnaire,
                Question: question,
                Group: group,
                MenuItem: menuItem,
                AnswerOption: answerOption
            };

        model.setDataContext = function (dc) {
            model.MenuItem.datacontext(dc);
            model.Questionnaire.datacontext(dc);
            model.Group.datacontext(dc);
            model.Question.datacontext(dc);
        };

        return model;
    });
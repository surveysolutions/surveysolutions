define('model',
    [
        'model.question',
        'model.group',
        'model.menuItem',
        'model.answerOption'
    ],
function (question, group, menuItem, answerOption) {
        var
            model = {
                Question: question,
                Group: group,
                MenuItem: menuItem,
                AnswerOption: answerOption
            };

        model.setDataContext = function (dc) {
            model.MenuItem.datacontext(dc);
            model.Group.datacontext(dc);
            model.Question.datacontext(dc);
        };

        return model;
    });
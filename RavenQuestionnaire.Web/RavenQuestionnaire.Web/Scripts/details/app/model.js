define('model',
    [
        'model.question',
        'model.group'
    ],
function (question, group) {
        var
            model = {
                Question: question,
                Group: group
            };

        model.setDataContext = function (dc) {
            // Model's that have navigation properties 
            // need a reference to the datacontext.
            model.Question.datacontext(dc);
            model.Group.datacontext(dc);
        };

        return model;
    });
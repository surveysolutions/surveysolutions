define('model',
    [
        'model.question',
        'model.group',
        'model.menuItem'
    ],
function (question, group, menuItem) {
        var
            model = {
                Question: question,
                Group: group,
                MenuItem: menuItem
            };

        model.setDataContext = function (dc) {
            model.MenuItem.datacontext(dc);
            //model.Group.datacontext(dc);
        };

        return model;
    });
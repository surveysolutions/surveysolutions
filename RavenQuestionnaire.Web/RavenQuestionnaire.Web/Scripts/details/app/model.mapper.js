define('model.mapper',
    ['model'],
    function(model) {
        var
            getAllGroups = function (questionnaire) {
                var groups = [];
               
                var stack = _.filter(questionnaire.Children, { '__type': 'GroupView' }).map(function(item) {
                    return { level: 0, group: item };
                });
                while (stack.length > 0) {
                    var item = stack.pop();
                    groups.push(item);
                    _.filter(item.group.Children, { '__type': 'GroupView' }).forEach(function(g) {
                        stack.push({ level: item.level + 1, group: g });
                    });
                }
                return groups;
            },
            menuItem = {
                getDtoId: function (dto) { return dto.group.PublicKey; },
                fromDto: function(dto, item) {
                    item = item || new model.MenuItem().id(dto.group.PublicKey).title(dto.group.Title).level(dto.level);
                    return item;
                },
                objectsFromDto: function(dto) {
                    return getAllGroups(dto);
                }
            },
            group = {
                getDtoId: function(dto) { return dto.id; },
                fromDto: function(dto, item) {
                    item = item || new model.Grop().id(dto.id);
                    return item;
                }
            },
            question = {
                getDtoId: function(dto) { return dto.id; },
                fromDto: function(dto, item) {
                    item = item || new model.Question().id(dto.id);
                    return item;
                }
            };

        return {
            question: question,
            group: group,
            menuItem: menuItem
        };
    });
define('model.mapper',
    ['model', 'config'],
    function (model, config) {
        var
            // private methods
            getGroups = function (group, level) {
                var items = _.filter(group.Children, { '__type': 'GroupView' }).map(function(item) {
                    return { level: level, group: item };
                });
                var groups = [];
                for (var i = items.length - 1; i >= 0; i--) {
                    groups.push(items[i]);
                }
                return groups;
            },
            getAllGroups = function(questionnaire) {
                var groups = [];
                var stack = getGroups(questionnaire, 0);
                while (stack.length > 0) {
                    var item = stack.pop();
                    groups.push(item);
                    _.forEach(getGroups(item.group, item.level + 1), function (g) {
                        stack.push(g);
                    });
                }
                return groups;
            },
            getAllQuestions = function (questionnaire) {
                console.log(questionnaire);
                var questions = [];
                var stack = getGroups(questionnaire, 0);
                while (stack.length > 0) {
                    var item = stack.pop();
                    _.filter(item.group.Children, { '__type': 'QuestionView' }).map(function (q) {
                        questions.push(q);
                    });

                    _.forEach(getGroups(item.group, item.level + 1), function (g) {
                        stack.push(g);
                    });
                }
                return questions;
            },
            // public mapping methods
            menuItem = {
                getDtoId: function(dto) { return dto.group.PublicKey; },
                fromDto: function(dto, item) {
                    item = item || new model.MenuItem().id(dto.group.PublicKey).title(dto.group.Title).level(dto.level);
                    return item;
                },
                objectsFromDto: function(dto) {
                    return getAllGroups(dto);
                }
            },
            group = {
                getDtoId: function(dto) { return dto.group.PublicKey; },
                fromDto: function(dto, item) {
                    item = item || new model.Group().id(dto.group.PublicKey).level(dto.level);
                    item.title(dto.group.Title);
                    item.type('type');
                    item.childrenID(_.map(dto.group.Children, function (c) {
                        return { type: c.__type, id: c.PublicKey };
                    }));
                    return item;
                },
                objectsFromDto: function (dto) {
                    return getAllGroups(dto);
                }
            },
            question = {
                getDtoId: function (dto) { return dto.PublicKey; },
                fromDto: function(dto, item) {
                    item = item || new model.Question().id(dto.PublicKey).title(dto.Title);

                    var type = config.questionTypes[dto.QuestionType];
                    item.type(type);
                    
                    item.answerOrder(dto.AnswerOrder);
                    item.answerOptions(dto.Answers);
                    item.isHead(dto.Capital);
                    item.isFeatured(dto.Featured);
                    item.isMandatory(dto.Mandatory);
                    item.cards(dto.Cards);
                    item.condition(dto.ConditionExpression);
                    item.instruction(dto.Instructions);
                    item.maxValue(dto.MaxValue);
                    item.scope(dto.QuestionScope);
                    
                    item.alias(dto.StataExportCaption);
                    item.triggers(dto.Triggers);
                    item.validationExpression(dto.ValidationExpression);
                    item.validationMessage(dto.ValidationMessage);
                    
                    return item;
                },
                objectsFromDto: function (dto) {
                    return getAllQuestions(dto);
                }
            };

        return {
            question: question,
            group: group,
            menuItem: menuItem
        };
    });
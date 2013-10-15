﻿define('model.mapper',
    ['model', 'config'],
    function (model, config) {
        var
        // private methods
            getGroups = function (group, level) {
                var items = _.filter(group.Children, { '__type': 'GroupView' }).map(function (item) {
                    return { level: level, group: item };
                });
                var groups = [];
                for (var i = items.length - 1; i >= 0; i--) {
                    groups.push(items[i]);
                }
                return groups;
            },
            getAllGroups = function (questionnaire) {
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
             questionnaire = {
                 getDtoId: function (dto) { return dto.PublicKey; },
                 fromDto: function (dto, item) {
                     item = item || new model.Questionnaire().id(dto.PublicKey).title(dto.Title).isPublic(dto.IsPublic);
                     item.childrenID(_.map(dto.Children, function (c) {
                         return { type: c.__type, id: c.PublicKey };
                     }));
                     item.dirtyFlag().reset();
                     return item;
                 }
             },
            group = {
                getDtoId: function (dto) { return dto.group.PublicKey; },
                fromDto: function (dto, item) {
                    item = item || new model.Group().id(dto.group.PublicKey).level(dto.level);
                    item.title(dto.group.Title);
                    item.parent(null);
                    item.description(dto.group.Description);
                    item.condition(dto.group.ConditionExpression);
                    item.gtype(dto.group.Propagated);

                    item.childrenID(_.map(dto.group.Children, function (c) {
                        return { type: c.__type, id: c.PublicKey };
                    }));

                    item.isNew(false);
                    item.dirtyFlag().reset();
                    item.commit();
                    return item;
                },
                objectsFromDto: function (dto) {
                    return getAllGroups(dto);
                }
            },
            question = {
                getDtoId: function (dto) { return dto.PublicKey; },
                fromDto: function (dto, item, otherData) {
                    var groups = otherData.groups;
                    item = item || new model.Question().id(dto.PublicKey).title(dto.Title);
                    item.parent(null);
                    item.qtype(dto.QuestionType);

                    if (dto.Featured == false) {
                        item.scope(dto.QuestionScope);
                    }

                    item.answerOrder(dto.AnswerOrder);

                    var answers = _.map(dto.Answers, function (answer) {
                        return new model.AnswerOption().id(answer.PublicKey).title(answer.Title).value(answer.AnswerValue);
                    });

                    var triggers = _.filter(dto.Triggers, function (groupId) {
                        var item = groups.getLocalById(groupId);
                        return !_.isNull(item);
                    }).map(function (groupId) {
                        return { key: groupId, value: groups.getLocalById(groupId).title() };
                    });

                    _.map(dto.Triggers, function (groupId) {
                        var item = groups.getLocalById(groupId);
                        if (!_.isNull(item)) {
                            return { key: groupId, value: groups.getLocalById(groupId).title() };
                        }
                        return;
                    });
                    item.triggers(triggers);

                    item.answerOptions(answers);
                    item.isHead(dto.Capital);
                    item.isFeatured(dto.Featured);
                    item.isMandatory(dto.Mandatory);
                    item.cards(dto.Cards);
                    item.condition(dto.ConditionExpression);
                    item.instruction(dto.Instructions);
                    item.maxValue(dto.MaxValue);

                    item.alias(dto.StataExportCaption);

                    item.validationExpression(dto.ValidationExpression);
                    item.validationMessage(dto.ValidationMessage);
                    item.selectedLinkTo(dto.LinkedToQuestionId);
                    item.isLinked(_.isNull(dto.LinkedToQuestionId) == false ? 1 : 0);
                    item.isInteger(_.isNull(dto.IsInteger) ? 0 : (dto.IsInteger ? 1 : 0));
                    item.countOfDecimalPlaces(dto.CountOfDecimalPlaces);

                    item.isNew(false);
                    item.dirtyFlag().reset();
                    item.commit();
                    
                    if (dto.Featured && dto.QuestionScope == config.questionScopes.supervisor) {
                    //    item.isNew(true);
                    }
                    
                    return item;
                },
                objectsFromDto: function (dto) {
                    return getAllQuestions(dto);
                }
            };

        return {
            questionnaire: questionnaire,
            question: question,
            group: group
        };
    });
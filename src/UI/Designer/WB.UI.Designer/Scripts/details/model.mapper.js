define('model.mapper',
    ['model', 'config'],
    function(model, config) {
        var getType = function(intType) {
            return intType === 1 ? "QuestionView" : "GroupView";
        }
        // public mapping methods
            error = {
                getDtoId: function (dto) { return dto.Code; },
                fromDto: function (dto) {
                    return new model.Error(dto.Message, dto.Code, _.map(dto.References, function (reference) {
                        return {
                            type: reference.Type,
                            id: reference.Id
                        };
                    }));
                }
            },
        questionnaire = {
            getDtoId: function(dto) { return dto.Id; },
            fromDto: function(dto, item) {
                item = item || new model.Questionnaire();
                item.id(this.getDtoId(dto));
                item.title(dto.Title);
                item.isPublic(dto.IsPublic);
                item.childrenID(_.map(dto.Chapters, function(c) {
                    return { type: getType(c.Type), id: c.Id };
                }));
                item.dirtyFlag().reset();
                return item;
            }
        },
        group = {
            getDtoId: function(dto) { return dto.Id; },
            fromDto: function(dto, item) {
                item = item || new model.Group();
                item.id(this.getDtoId(dto));
                item.level(dto.Level);
                item.title(dto.Title);
                item.parent(dto.ParentId);
                item.description(dto.Description);
                item.condition(dto.ConditionExpression);
                item.gtype(dto.Propagated);

                item.childrenID(_.map(dto.Children, function(c) {
                    return { type: getType(c.Type), id: c.Id };
                }));

                item.isNew(false);
                item.dirtyFlag().reset();
                item.commit();
                return item;
            }
        },
        question = {
            getDtoId: function(dto) { return dto.Id; },
            fromDto: function(dto, item, otherData) {
                var groups = otherData.groups;
                item = item || new model.Question();
                item.id(dto.PublicKey);
                item.title(dto.Title);
                item.parent(null);
                if (!_.isEmpty(dto.ParentId)) {
                    item.parent(groups.getLocalById(dto.ParentId));
                }
                item.qtype(dto.QuestionType);

                if (dto.Featured == false) {
                    item.scope(dto.QuestionScope);
                }

                item.answerOrder(dto.AnswerOrder);

                var answers = _.map(dto.Answers, function(answer) {
                    return new model.AnswerOption().id(answer.PublicKey).title(answer.Title).value(answer.AnswerValue);
                });

                var triggers = _.filter(dto.Triggers, function(groupId) {
                    var item = groups.getLocalById(groupId);
                    return !_.isEmpty(item);
                }).map(function(groupId) {
                    return { key: groupId, value: groups.getLocalById(groupId).title() };
                });

                _.map(dto.Triggers, function(groupId) {
                    var item = groups.getLocalById(groupId);
                    if (!_.isEmpty(item)) {
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
                item.isLinked(_.isEmpty(dto.LinkedToQuestionId) == false ? 1 : 0);
                item.isInteger(_.isEmpty(dto.IsInteger) ? 0 : (dto.IsInteger ? 1 : 0));
                item.countOfDecimalPlaces(_.isEmpty(dto.Settings) ? null : dto.Settings.CountOfDecimalPlaces);

                    item.areAnswersOrdered(_.isEmpty(dto.Settings) ? false : dto.Settings.AreAnswersOrdered);
                    item.maxAllowedAnswers(_.isEmpty(dto.Settings) ? null : dto.Settings.MaxAllowedAnswers);
                    
                item.isNew(false);
                item.dirtyFlag().reset();
                item.commit();

                return item;
            }
        };

        return {
            questionnaire: questionnaire,
            question: question,
            group: group,
            error: error
        };
    });
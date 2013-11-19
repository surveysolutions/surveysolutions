define('model.mapper',
    ['model', 'config'],
    function (model, config) {
        var getType = function (intType) {
            return intType === 1 ? "QuestionView" : "GroupView";
        },
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
                getDtoId: function (dto) { return dto.Id; },
                fromDto: function (dto, item) {
                    item = item || new model.Questionnaire();
                    item.id(this.getDtoId(dto));
                    item.title(dto.Title);
                    item.isPublic(dto.IsPublic);
                    item.childrenID(_.map(dto.Chapters, function (c) {
                        return { type: getType(c.Type), id: c.Id };
                    }));
                    item.dirtyFlag().reset();
                    return item;
                }
            },
            group = {
                getDtoId: function (dto) { return dto.Id; },
                fromDto: function (dto, item) {
                    item = item || new model.Group();
                    item.id(this.getDtoId(dto));
                    item.level(dto.Level);
                    item.title(dto.Title);
                    item.parent(dto.ParentId);
                    item.description(dto.Description);
                    item.condition(dto.ConditionExpression);
                    item.isRoster(dto.IsRoster);
                    item.rosterSizeQuestion(dto.RosterSizeQuestionId);

                    item.childrenID(_.map(dto.Children, function (c) {
                        return { type: getType(c.Type), id: c.Id };
                    }));

                    item.isNew(false);
                    item.dirtyFlag().reset();
                    item.commit();
                    return item;
                }
            },
            question = {
                getDtoId: function (dto) { return dto.Id; },
                fromDto: function (dto, item, otherData) {
                    var groups = otherData.groups;
                    item = item || new model.Question();
                    item.id(this.getDtoId(dto));
                    item.title(dto.Title);
                    item.parent(null);
                    if (!_.isEmpty(dto.ParentId)) {
                        item.parent(groups.getLocalById(dto.ParentId));
                    }
                    item.qtype(dto.QuestionType);

                    if (dto.Featured == false) {
                        item.scope(dto.QuestionScope);
                    }
                  
                    item.isHead(dto.Capital);
                    item.isFeatured(dto.Featured);
                    item.isMandatory(dto.Mandatory);
                    item.condition(dto.ConditionExpression);
                    item.instruction(dto.Instructions);

                    item.alias(dto.Alias);

                    item.validationExpression(dto.ValidationExpression);
                    item.validationMessage(dto.ValidationMessage);

                    if (!_.isEmpty(dto.Settings)) {
                        var settings = dto.Settings;
                        
                        item.isLinked(_.isEmpty(settings.LinkedToQuestionId) == false ? 1 : 0);
                        item.selectedLinkTo(settings.LinkedToQuestionId);

                        item.answerOptions(_.map(settings.Answers, function (answer) {
                            return new model.AnswerOption()
                                .id(answer.Id)
                                .title(answer.Title)
                                .value(answer.AnswerValue);
                        }));
                       
                        item.isInteger(_.isBoolean(settings.IsInteger) ? (settings.IsInteger ? 1 : 0) : 0);
                        item.maxValue(_.isNumber(settings.MaxValue) ? settings.MaxValue * 1 : null);
                        item.countOfDecimalPlaces(_.isEmpty(settings.CountOfDecimalPlaces) ? null : settings.CountOfDecimalPlaces);
                        item.areAnswersOrdered(_.isBoolean(dto.Settings.AreAnswersOrdered) ? settings.AreAnswersOrdered : false);
                        item.maxAllowedAnswers(_.isNumber(dto.Settings.MaxAllowedAnswers) ? settings.MaxAllowedAnswers: null);
                    }
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
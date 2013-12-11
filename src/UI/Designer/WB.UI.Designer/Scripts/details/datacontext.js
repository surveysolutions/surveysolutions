define('datacontext',
    ['jquery', 'underscore', 'ko', 'model', 'config', 'dataservice', 'model.mapper', 'utils', 'input'],
    function ($, _, ko, model, config, dataservice, modelmapper, utils, input) {

        var 
            itemsToArray = function (items, observableArray, filter, sortFunction) {
                // Maps the memo to an observableArray, 
                // then returns the observableArray
                if (!observableArray) return;

                // Create an array from the memo object
                var underlyingArray = utils.mapMemoToArray(items);

                if (filter) {
                    underlyingArray = _.filter(underlyingArray, function (o) {
                        var match = filter.predicate(filter, o);
                        return match;
                    });
                }
                if (sortFunction) {
                    underlyingArray.sort(sortFunction);
                }
                //logger.info('Fetched, filtered and sorted ' + underlyingArray.length + ' records');
                observableArray(underlyingArray);
            },
            mapToContext = function (dtoList, items, results, mapper, filter, sortFunction, otherData) {
                // Loop through the raw dto list and populate a dictionary of the items
                items = _.reduce(dtoList, function (memo, dto) {
                    var id = mapper.getDtoId(dto);
                    var existingItem = items[id];
                    memo[id] = mapper.fromDto(dto, existingItem, otherData);
                    return memo;
                }, {});
                itemsToArray(items, results, filter, sortFunction);
                //logger.success('received with ' + dtoList.length + ' elements');
                return items; // must return these
            },
            LocalEntitySet = function (mapper, nullo, otherData) {
                var items = {},
                    // returns the model item produced by merging dto into context
                    mapDtoToContext = function (dto) {
                        var id = mapper.getDtoId(dto);
                        var existingItem = items[id];
                        items[id] = mapper.fromDto(dto, existingItem, otherData);
                        return items[id];
                    },
                    add = function (newObj) {
                        items[newObj.id()] = newObj;
                    },
                    removeById = function (id) {
                        delete items[id];
                    },
                    getLocalById = function (id) {
                        // This is the only place we set to NULLO
                        return !!id && !!items[id] ? items[id] : null;
                    },
                    getAllLocal = function () {
                        return utils.mapMemoToArray(items);
                    },
                    parse = function (data, options) {
                        return $.Deferred(function (def) {
                            var results = options && options.results,
                                sortFunction = options && options.sortFunction,
                                filter = options && options.filter,
                                forceRefresh = (options && options.forceRefresh) || false;

                            if (forceRefresh || !items || !utils.hasProperties(items)) {
                                items = mapToContext(data, items, results, mapper, filter, sortFunction, otherData);
                                def.resolve(results);
                            } else {
                                itemsToArray(items, results, filter, sortFunction);
                                def.resolve(results);
                            }
                        }).promise();
                    };

                return {
                    mapDtoToContext: mapDtoToContext,
                    add: add,
                    getAllLocal: getAllLocal,
                    getLocalById: getLocalById,
                    parse: parse,
                    removeById: removeById
                };
            },
            //----------------------------------
            // Repositories
            //
            // Pass: 
            //  dataservice's 'get' method
            //  model mapper
            //----------------------------------

            groups = new LocalEntitySet(modelmapper.group, model.Group.Nullo),
            questions = new LocalEntitySet(modelmapper.question, model.Question.Nullo, { groups: groups }),
            questionnaire = modelmapper.questionnaire.fromDto(input.questionnaire);


        groups.parse(input.questionnaire.Groups);
        questions.parse(input.questionnaire.Questions);

        //input.questionnaire = null;

        // set parents
        _.each(groups.getAllLocal(), function (group) {
            group.parent(groups.getLocalById(group.parent()));
        });

        groups.search = function (query) {
            var items = _.filter(groups.getAllLocal(), function (item) {
                return item.title().toLowerCase().indexOf(query) !== -1;
            });
            return items;
        };

        groups.removeGroup = function (id) {
            var group = groups.getLocalById(id);
            _.each(group.childrenID(), function (item) {
                if (item.type === "GroupView")
                    return groups.removeGroup(item.id);
                return questions.removeById(item.id);
            });
            groups.removeById(id);
        };

        groups.getChapters = function () {
            var chapters = _.map(questionnaire.childrenID(), function (children) {
                var item = groups.getLocalById(children.id);
                //item.parent(parent);
                return item;
            });
            //_.filter(groups.getAllLocal(), function (item) {
            //    return item.level() == 0;
            //});
            return chapters;
        };

        groups.getPropagateableGroups = function () {
            var propagatable = _.filter(groups.getAllLocal(), function (item) {
                return item.isRoster();
            });
            return propagatable;
        };

        groups.getQuestionsFromPropagatableGroups = function () {
            return _.filter(questions.getAllLocal(), function (item) {
                return !_.isUndefined(item.parent()) && !_.isNull(item.parent()) && item.parent().isRoster();
            });
        };

        questions.search = function (query) {
            var items = _.filter(questions.getAllLocal(), function (item) {
                return item.title().toLowerCase().indexOf(query) !== -1;
            });
            return items;
        };

        questions.getLocalByVariable = function (variable) {
            return _.find(questions.getAllLocal(), function (question) { return question.alias() == variable; });
        };

        questions.cleanTriggers = function (group) {
            _.each(questions.getAllLocal(), function (question) {
                var child = _.find(question.triggers(), { 'key': group.id });
                if (!_.isUndefined(child)) {
                    var isDirty = question.dirtyFlag().isDirty();
                    question.triggers.remove(child);
                    if (!isDirty) {
                        question.dirtyFlag().reset();
                    }
                }
            });
        };

        questions.getAllVariables = function () {
            return _.map(questions.getAllLocal(), function (question) {
                return question.alias();
            });
        };
        
        questions.getAllAllowedQuestionsForSelect = function () {
            return _.filter(questions.getAllLocal(), function (question) {
                if (question.parent().isRoster()) {
                    return false;
                }
                return isNumericInteger(question) || isNotLinkedMultyCategorical(question);
            }).map(function (item) {
                return { questionId: item.id(), title: item.alias() + ": " + item.title() };
            });
        };

        var isNumericInteger = function(question) {
            return question.qtype() == config.questionTypes.Numeric && question.isInteger() == 1;
        };

        var isNotLinkedMultyCategorical = function (question) {
            return question.qtype() == config.questionTypes.MultyOption && !question.isLinked();
        };

        var getChildItemByIdAndType = function (item) {
            if (item.type === "GroupView")
                return groups.getLocalById(item.id);
            return questions.getLocalById(item.id);
        };

        var firstSavedIndexInCollection = function (collection, id) {
            var item = utils.findById(collection, id);
            for (var i = item.index; i >= 0; i--) {
                var child = getChildItemByIdAndType(collection[i]);
                if (!child.isNew())
                    return i + 1;
            }
            return 0;
        };

        var commands = {};


        commands[config.commands.updateQuestionnaire] = function (questionnaire) {
            return {
                questionnaireId: questionnaire.id(),
                title: questionnaire.title(),
                isPublic: questionnaire.isPublic()
            };
        };

        commands[config.commands.cloneGroup] = function (group) {
            var command = commands[config.commands.createGroup](group);
            command.sourceGroupId = group.cloneSource().id();
            command.targetIndex = firstSavedIndexInCollection(group.hasParent() ? group.parent().childrenID() : questionnaire.childrenID(), group.id());
            return command;
        };

        commands[config.commands.deleteGroup] = function (group) {
            return {
                questionnaireId: questionnaire.id(),
                groupId: group.id()
            };
        };

        commands[config.commands.createGroup] = function (group) {
            var parent = group.parent();
            if (!_.isNull(parent))
                parent = parent.id();

            var groupCommand = converGroupToCommand(group);
            groupCommand.parentGroupId = parent;
            return groupCommand;
        };
        
        commands[config.commands.updateGroup] = function (group) {
            return converGroupToCommand(group);
        };

        var converGroupToCommand = function(group) {
            return {
                questionnaireId: questionnaire.id(),
                groupId: group.id(),
                title: group.title(),
                description: group.description(),
                condition: group.condition(),
                rosterSizeQuestionId: group.isRoster() ? group.rosterSizeQuestion() : null
            };
        };

        commands[config.commands.cloneQuestion] = function (question) {
            var command = commands[config.commands.createQuestion](question);
            command.sourceQuestionId = question.cloneSource().id();
            command.targetIndex = firstSavedIndexInCollection(question.parent().childrenID(), question.id());
            return command;
        };

        commands[config.commands.createQuestion] = function (question) {
            var command = converQuestionToCommand(question);
            command.groupId = question.parent().id();
            return command;
        };

        commands[config.commands.updateQuestion] = function (question) {
            return converQuestionToCommand(question);
        };

        commands[config.commands.cloneNumericQuestion] = function (question) {
            var command = commands[config.commands.createNumericQuestion](question);
            command.sourceQuestionId = question.cloneSource().id();
            command.targetIndex = firstSavedIndexInCollection(question.parent().childrenID(), question.id());
            return command;
        };

        commands[config.commands.createNumericQuestion] = function (question) {
            var command = converQuestionToCommand(question);
            command.groupId = question.parent().id();
            return command;
        };

        commands[config.commands.updateNumericQuestion] = function (question) {
            return converQuestionToCommand(question);
        };

        commands[config.commands.deleteQuestion] = function (question) {
            return {
                questionnaireId: questionnaire.id(),
                questionId: question.id()
            };
        };

        commands[config.commands.questionMove] = function (command) {
            command.questionnaireId = questionnaire.id();
            return command;
        };

        commands[config.commands.groupMove] = function (command) {
            command.questionnaireId = questionnaire.id();
            return command;
        };

        commands[config.commands.addSharedPersonToQuestionnaire] = function (sharedUser) {
            return {
                email: sharedUser.userEmail(),
                questionnaireId: questionnaire.id()
            };
        };

        commands[config.commands.removeSharedPersonFromQuestionnaire] = function (sharedUser) {
            return {
                email: sharedUser.userEmail(),
                questionnaireId: questionnaire.id()
            };
        };

        var converQuestionToCommand = function (question) {
            var command = {
                questionnaireId: questionnaire.id(),

                questionId: question.id(),
                title: question.title(),
                type: question.qtype(),
                alias: question.alias(),
                isHeaderOfPropagatableGroup: question.isHead(),
                isFeatured: question.isFeatured(),
                isMandatory: question.isMandatory(),
                scope: question.scope(),
                condition: question.condition(),
                validationExpression: question.validationExpression(),
                validationMessage: question.validationMessage(),
                instructions: question.instruction()
            };
            switch (command.type) {
                case "SingleOption":
                case "YesNo":
                case "DropDownList":
                case "MultyOption":
                    command.areAnswersOrdered = question.areAnswersOrdered();
                    command.maxAllowedAnswers = question.maxAllowedAnswers();
                    if (question.isLinked() == 1) {
                        command.linkedToQuestionId = question.selectedLinkTo();
                    } else {
                        command.options = _.map(question.answerOptions(), function (item) {
                            return {
                                id: item.id(),
                                title: item.title(),
                                value: item.value()
                            };
                        });
                    }
                    break;
                case "Numeric":
                    command.isInteger = question.isInteger() == 1 ? true : false;
                    command.isAutopropagating = false;
                    command.countOfDecimalPlaces = command.isInteger == false ? question.countOfDecimalPlaces() : null;
                    command.maxValue = question.maxValue();
                case "DateTime":
                case "GpsCoordinates":
                case "Text":
                    break;
            }
            return command;
        };

        var runRemoteVerification = function (uiCallbacks) {
            return $.Deferred(function (deferred) {
                var callbacks = {
                    success: function (response) {
                        if (uiCallbacks && uiCallbacks.success) {
                            uiCallbacks.success(_.map(response.Errors, function (error) {
                                return modelmapper.error.fromDto(error);
                            }));
                        }
                        deferred.resolve(response);
                    },
                    error: function (response) {
                        if (uiCallbacks && uiCallbacks.error) {
                            uiCallbacks.error(response);
                        }
                        deferred.reject(response);
                    }
                };

                dataservice.runRemoteVerification(callbacks, questionnaire.id());

            }).promise();
        };
        var sendCommand = function(commandName, args, uiCallbacks) {
            return $.Deferred(function (deferred) {
                var command = {
                    type: commandName,
                    command: ko.toJSON(commands[commandName](args))
                }, callbacks = {
                    success: function(response) {
                        if (uiCallbacks && uiCallbacks.success) {
                            uiCallbacks.success();
                        }
                        deferred.resolve(response);
                    },
                    error: function(response) {
                        if (uiCallbacks && uiCallbacks.error) {
                            uiCallbacks.error(response);
                        }
                        deferred.reject(response);
                    }
                };
                dataservice.sendCommand(callbacks, command);
            }).promise();
        };

        var datacontext = {
            groups: groups,
            questions: questions,
            questionnaire: questionnaire,
            sendCommand: sendCommand,
            runRemoteVerification: runRemoteVerification
        };

        // We did this so we can access the datacontext during its construction
        model.setDataContext(datacontext);

        _.each(groups.getAllLocal(), function (group) {
            group.fillChildren();
        });

        return datacontext;
    });
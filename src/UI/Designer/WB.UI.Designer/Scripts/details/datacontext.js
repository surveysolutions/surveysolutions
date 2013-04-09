define('datacontext',
    ['jquery', 'underscore', 'ko', 'model', 'config', 'dataservice', 'model.mapper', 'utils', 'input'],
    function ($, _, ko, model, config, dataservice, modelmapper, utils, input) {

        var stack = [input.questionnaire];
        while (stack.length > 0) {
            var item = stack.pop();
            var type = item['$type'].split(",")[0];
            item["__type"] = type.substring(type.lastIndexOf('.') + 1);
            _.each(item.Children, function (q) {
                stack.push(q);
            });
        }


        var logger = config.logger,
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
                        return !!id && !!items[id] ? items[id] : nullo;
                    },
                    getAllLocal = function () {
                        return utils.mapMemoToArray(items);
                    },
                    parse = function (data, options) {
                        return $.Deferred(function (def) {
                            var results = options && options.results,
                                sortFunction = options && options.sortFunction,
                                filter = options && options.filter,
                                forceRefresh = options && options.forceRefresh;

                            var dtos = mapper.objectsFromDto(data);

                            // If the internal items object doesnt exist, 
                            // or it exists but has no properties, 
                            // or we force a refresh
                            if (forceRefresh || !items || !utils.hasProperties(items)) {
                                items = mapToContext(dtos, items, results, mapper, filter, sortFunction, otherData);
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


        groups.parse(input.questionnaire);
        questions.parse(input.questionnaire);

        // set parents
        _.each(groups.getAllLocal(), function (parent) {
            _.each(parent.childrenID(), function (children) {
                var item = (children.type === "GroupView") ? groups.getLocalById(children.id) : questions.getLocalById(children.id);
                item.parent(parent);
            });
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
            var chapters = _.filter(groups.getAllLocal(), function (item) {
                return item.level() == 0;
            });
            return chapters;
        };

        groups.getPropagateableGroups = function () {
            var propagatable = _.filter(groups.getAllLocal(), function (item) {
                return item.gtype() !== "None";
            });
            return propagatable;
        };

        questions.search = function (query) {
            var items = _.filter(questions.getAllLocal(), function (item) {
                return item.title().toLowerCase().indexOf(query) !== -1;
            });
            return items;
        };
        
        questions.cleanTriggers = function (group) {
            _.each(questions.getAllLocal(), function (question) {
                var child = _.find(question.triggers(), { 'key': group.id });
                if (!_.isUndefined(child)) {
                    question.triggers.remove(child);
                }
            });
        };

        var commands = {};

        commands[config.commands.createGroup] = function (group) {
            var parent = group.parent();
            if (!_.isNull(parent))
                parent = parent.id();

            return {
                questionnaireId: questionnaire.id(),
                groupId: group.id(),
                title: group.title(),
                propagationKind: group.gtype(),
                description: group.description(),
                condition: group.condition(),
                parentGroupId: parent
            };
        };

        commands[config.commands.deleteGroup] = function (group) {
            return {
                questionnaireId: questionnaire.id(),
                groupId: group.id()
            };
        };

        commands[config.commands.updateGroup] = function (group) {
            return {
                questionnaireId: questionnaire.id(),
                groupId: group.id(),
                title: group.title(),
                propagationKind: group.gtype(),
                description: group.description(),
                condition: group.condition()
            };
        };

        commands[config.commands.createQuestion] = function (question) {
            var command = converQuestionToCommand(question);
            command.groupId = question.parent().id();
            return command;
        };

        commands[config.commands.deleteQuestion] = function (question) {
            return {
                questionnaireId: questionnaire.id(),
                questionId: question.id()
            };
        };

        commands[config.commands.updateQuestion] = function (question) {
            return converQuestionToCommand(question);
        };
        
        commands[config.commands.questionMove] = function (command) {
            command.questionnaireId = questionnaire.id();
            return command;
        };
        
        commands[config.commands.groupMove] = function (command) {
            command.questionnaireId = questionnaire.id();
            return command;
        };

        var converQuestionToCommand = function(question) {
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
                command.optionsOrder = question.answerOrder();
                command.options = _.map(question.answerOptions(), function(item) {
                    return {
                        id: item.id(),
                        title: item.title(),
                        value: item.value()
                    };
                });
                break;
            case "Numeric":
            case "DateTime":
            case "GpsCoordinates":
            case "Text":
                break;
            case "AutoPropagate":
                command.maxValue = question.maxValue();
                command.triggedGroupIds = _.map(question.triggers(), function(trriger) {
                    return trriger.key;
                });
                break;
            }
            return command;
        };
        
        var sendCommand = function (commandName, args, callbacks) {
            return $.Deferred(function (def) {
                var command = {
                    type: commandName,
                    command: ko.toJSON(commands[commandName](args))
                };

                dataservice.sendCommand({
                    success: function (response, status) {
                        if (callbacks && callbacks.success) {
                            callbacks.success();
                        }
                        def.resolve(response);
                    },
                    error: function (response, xhr) {
                        if (callbacks && callbacks.error) {
                            callbacks.error(response);
                        }
                        def.reject(response);
                        return;
                    }
                }, command);
            }).promise();
        };

        var datacontext = {
            groups: groups,
            questions: questions,
            questionnaire: questionnaire,
            sendCommand: sendCommand
        };

        // We did this so we can access the datacontext during its construction
        model.setDataContext(datacontext);

        _.each(groups.getAllLocal(), function (group) {
            group.fillChildren();
        });

        return datacontext;
    });
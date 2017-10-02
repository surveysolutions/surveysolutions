DataContext = function (mapper) {
    var EntitySet = function (mapper) {
        var items = {},
            mapDtoToContext = function (index, dto) {
                var id = mapper.getDtoId(dto);
                items[id] = mapper.fromDto(dto);
                return items[id];
            },
            getLocalById = function (id) {
                return !!id && !!items[id] ? items[id] : null;
            },
            getAllLocal = function () {
                return Supervisor.Framework.Objects.Values(items);
            },
            getData = function (dtos) {
                return $.Deferred(function (def) {
                    if (!items || Supervisor.Framework.Objects.isEmpty(items)) {
                        $.each(dtos, mapDtoToContext);
                        def.resolve(getAllLocal());
                    } else {
                        def.resolve(getAllLocal());
                    }
                }).promise();
            };

        return {
            mapDtoToContext: mapDtoToContext,
            getAllLocal: getAllLocal,
            getLocalById: getLocalById,
            getData: getData
        };
    },
        questions = new EntitySet(mapper.question),
        status = {},
        responsible = {},
        questionnaire = {};

    function hasValue(input) {
        return !_.isUndefined(input) && input !== "";
    }

    var prepareQuestion = function () {
        var answers = $.map(questions.getAllLocal(), function (question) {
            var answer = null;
            switch (question.type()) {
                case "Text":
                    if (hasValue(question.selectedOption())) {
                        var inputValue = question.selectedOption().split(',').join('');

                        if (_.isEmpty(inputValue))
                            break;

                        if (!_.isEmpty(question.mask())) {
                            // doesn't match the mask
                            var reqexpMask = question.mask().replace(new RegExp("\#", 'g'), "[0-9]");
                            reqexpMask = reqexpMask.replace(new RegExp("\~", 'g'), "[A-Za-z]");
                            reqexpMask = reqexpMask.replace(new RegExp("[*]", 'g'), "[A-Za-z0-9]");

                            var matchResult = inputValue.match(new RegExp(reqexpMask)) || [];
                            if (matchResult.length === 0) {
                                break;
                            }

                            // is empty
                            var defautMask = question.mask();
                            defautMask = defautMask.replace(new RegExp("\#\~\*", 'g'), "_");
                            if (inputValue === defautMask)
                                break;
                        }

                        answer = {
                            id: question.id(),
                            answer: inputValue,
                            settings: question.settings(),
                            type: question.type()
                        };
                    }
                    break;
                case "Numeric":
                    if (hasValue(question.selectedOption())) {
                        answer = {
                            id: question.id(),
                            answer: question.selectedOption().split(',').join(''),
                            settings: question.settings(),
                            type: question.type()
                        };
                    }
                    break;
                case "DateTime":
                    var dateAnswer = question.selectedOption();
                    if (hasValue(dateAnswer) && !_.isNull(dateAnswer)) {
                        var date = new Date(dateAnswer);
                        answer = {
                            id: question.id(),
                            answer: date.getFullYear() +
                                '-' +
                                (date.getMonth() + 1) +
                                '-' +
                                date.getDate(),
                            type: question.type()
                        };

                        if (question.settings().IsTimestamp) {
                            answer.answer = answer.answer +
                                "T" +
                                date.getHours() +
                                ":" + 
                                date.getMinutes() +
                                ":" + 
                                date.getSeconds();
                        }
                    }
                    break;
                case "GpsCoordinates":
                    if (hasValue(question.latitude()) && hasValue(question.longitude())) {
                        answer = {
                            id: question.id(),
                            answer: question.latitude() + "$" + question.longitude(),
                            type: question.type()
                        };
                    }
                    break;
                case "SingleOption":
                    var singleAnswer = question.selectedOption();

                    if (hasValue(singleAnswer)) {
                        if (question.isFilteredCombobox())
                            singleAnswer = question.selectedOption().value();

                        if (hasValue(singleAnswer)) {
                            answer = {
                                id: question.id(),
                                answer: singleAnswer,
                                type: question.type()
                            };
                        }
                    }
                    break;
                case "MultyOption":
                    answer = {
                        id: question.id(),
                        answer: question.selectedOptions(),
                        type: question.type()
                    };
            }
            return answer;
        });
        return _.filter(answers, function(o) { return !_.isNull(o); });
    };

    var parseData = function (q) {
        status = q.Status;

        questionnaire.templateVersion = q.QuestionnaireVersion;
        questionnaire.templateId = q.QuestionnaireId;
        questionnaire.title = q.QuestionnaireTitle;

        responsible = q.Responsible;
        questions.getData(q.FeaturedQuestions);
    };

    return {
        questions: questions,
        questionnaire: questionnaire,
        status: status,
        parseData: parseData,
        prepareQuestion: prepareQuestion
    };
};
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

    var prepareQuestion = function () {
        var answers = $.map(questions.getAllLocal(), function (question) {
            var answer = {};
            switch (question.type()) {
                case "Text":
                case "AutoPropagate":
                case "Numeric":
                        answer = {
                            id: question.id(),
                            answer: question.selectedOption().split(',').join(''),
                            settings:  question.settings(),
                            type: question.type()
                        };
                        break;
                case "DateTime":
                    var dateAnswer = question.selectedOption();
                    var answerUTC = null;

                    if (question.settings().IsTimestamp) {
                        answerUTC = moment(dateAnswer);
                    } else {
                        answerUTC = dateAnswer.getFullYear() + '-' + (dateAnswer.getMonth() + 1) + '-' + dateAnswer.getDate();
                    }
                    answer = {
                        id: question.id(),
                        answer: answerUTC,
                        type: question.type()
                    };
                    break;
                case "GpsCoordinates":
                    answer = {
                        id: question.id(),
                        answer: question.latitude() + "$" + question.longitude(),
                        type: question.type()
                    };
                    break;
                case "SingleOption":
                    answer = {
                        id: question.id(),
                        answer: question.isFilteredCombobox() ? question.selectedOption().value() : question.selectedOption(),
                        type: question.type()
                    };
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
        return answers;
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
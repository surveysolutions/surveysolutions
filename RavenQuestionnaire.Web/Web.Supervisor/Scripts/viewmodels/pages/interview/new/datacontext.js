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
        supervisors = new EntitySet(mapper.user),
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
                            answer: question.selectedOption(),
                            settings:  question.settings(),
                            type: question.type()
                        };
                        break;
                case "DateTime":
                case "GpsCoordinates":
                case "SingleOption":
                    answer = {
                        id: question.id(),
                        answer: question.selectedOption(),
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

        questionnaire.id = Math.uuid();
        questionnaire.templateVersion = q.QuestionnaireVersion;
        questionnaire.templateId = q.QuestionnaireId;
        questionnaire.title = q.QuestionnaireTitle;

        responsible = q.Responsible;
        questions.getData(q.FeaturedQuestions);
        supervisors.getData(q.Supervisors);
    };

    return {
        questions: questions,
        questionnaire: questionnaire,
        status: status,
        supervisors: supervisors,
        parseData: parseData,
        prepareQuestion: prepareQuestion
    };
};
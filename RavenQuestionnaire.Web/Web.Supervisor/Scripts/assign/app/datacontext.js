define('app/datacontext',
    ['jquery', 'lodash', 'app/dataservice', 'app/mapper'],

    function ($, _, dataservice, mapper) {
        var EntitySet = function (mapper) {
            var items = {},
                mapDtoToContext = function (dto) {
                    var id = mapper.getDtoId(dto);
                    items[id] = mapper.fromDto(dto);
                    return items[id];
                },
                getLocalById = function (id) {
                    return !!id && !!items[id] ? items[id] : null;
                },
                getAllLocal = function () {
                    return _.values(items);
                },
                getData = function (dtos) {
                    return $.Deferred(function (def) {
                        if (!items || _.isEmpty(items)) {
                            _.each(dtos, mapDtoToContext);
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
         save = function (args, callbacks) {
             
             return $.Deferred(function (def) {

                 var answers = _.map(questions.getAllLocal(), function (question) {
                     
                     var answer = {
                         Id: question.id(),
                         Type: question.type(),
                         Answer: question.hasOptions() ? "" : question.selectedOption(),
                         Answers: []
                    };
                    
                     if (question.hasOptions()) {
                         if (question.type() == "SingleOption")
                             answer.Answers.push(question.selectedOption());
                         else
                             answer.Answers = question.selectedOptions();
                     }
                     
                     return answer;
                 });

                 var data = {
                     QuestionnaireId : questionnaire.id,
                     Responsible: {
                         Id:args.id,
                         Name: args.name
                     },
                     Answers: answers
                 };
                 
                 dataservice.sendAssingmentData({
                     success: function (response) {
                         if (callbacks && callbacks.success) {
                             callbacks.success(response);
                         }
                         def.resolve(response);
                     },
                     error: function (response) {
                         if (callbacks && callbacks.error) {
                             callbacks.error(response);
                         }
                         def.reject(response);
                         return;
                     }
                 }, data);
             }).promise();
         },
            questions = new EntitySet(mapper.question),
            supervisors = new EntitySet(mapper.user),
            status = {},
            responsible = {},
            questionnaire = {};

        var parseData = function (input) {
            status = input.questionnaire.Status;

            questionnaire.id = input.questionnaire.Id;
            questionnaire.templateId = input.questionnaire.TemplateId;
            questionnaire.title = input.questionnaire.QuestionnaireTitle;

            responsible = input.questionnaire.Responsible;
            questions.getData(input.questionnaire.FeaturedQuestions);
            supervisors.getData(input.questionnaire.Supervisors);
        };
        return {
            questions: questions,
            questionnaire: questionnaire,
            status: status,
            parseData: parseData,
            supervisors: supervisors,
            save: save
        };
    });
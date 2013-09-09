define('app/mapper', ['lodash', 'app/model', 'app/config'],
    function (_, model, config) {
        var question = {
            getDtoId: function (dto) { return dto.Id; },
            fromDto: function (dto) {
                var item = {};
                switch (dto.QuestionType) {
                    case 0:
                        item = new model.SingleOptionQuestion();
                        break;
                    case 3:
                        item = new model.MultyOptionQuestion();
                        break;
                    case 7:
                        item = new model.TextQuestion();
                        item.answer(dto.Answer);
                        break;
                    case 4:
                    case 8:
                        item = new model.NumericQuestion();
                        item.answer(dto.Answer * 1);
                        break;
                    case 5:
                        item = new model.DateTimeQuestion();
                        item.answer(new Date(dto.Answer));
                        break;
                    case 6:
                        item = new model.GpsQuestion();
                        break;
                }
                item.scope(dto.Scope);
                item.isAnswered(dto.IsAnswered);
                item.id(dto.Id);
                item.title(dto.Title);
                item.isFlagged(dto.IsFlagged);
                item.questionType(config.questionTypeMap[dto.QuestionType]);
                item.isCapital(dto.IsCapital);
                item.comments(dto.Comments);
                item.isEnabled(dto.IsEnabled);
                item.isFeatured(dto.IsFeatured);
                item.isMandatory(dto.IsMandatory);
                item.propagationVector(dto.PropagationVector);
                item.isValid(dto.IsValid);
                item.validationMessage(dto.ValidationMessage);
                item.validationExpression(dto.ValidationExpression);
                return item;
            }
        },
            group = {
                getDtoId: function (dto) { return dto.Id; },
                fromDto: function (dto, questions) {
                    var item = new model.Group();
                    item.id(dto.Id);
                    item.depth(dto.Depth);
                    item.parentId(dto.ParentId);
                    item.propagationVector(dto.PropagationVector);
                    item.questions(_.map(dto.Questions, function (q) {
                        return questions.getLocalById(question.getDtoId(q));
                    }));
                    item.title(dto.Title);
                    return item;
                }
            },
			interview = {
			    getDtoId: function (dto) { return dto.Id; },
			    fromDto: function (dto) {
			        var item = new model.Interview();
			        item.id(dto.PublicKey);
			        item.title(dto.Title);
			        item.status(dto.Status);
			        item.questionnaireId(dto.QuestionnairePublicKey);
			        return item;
			    }
			},
			user = {
			    getDtoId: function (dto) { return dto.Id; },
			    fromDto: function (dto) {
			        var item = new model.User();
			        item.id(dto.Id);
			        item.name(dto.Name);
			        return item;
			    }
			};
        return {
            question: question,
            group: group,
            interview: interview,
            user: user
        };
    });

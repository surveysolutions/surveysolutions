Mapper = function(model, config) {
    var question = {
        getDtoId: function(dto) { return dto.Id + "_" + dto.RosterVector; },
        fromDto: function(dto) {
            var item = {};
            var uiId = dto.Id + "_" + dto.RosterVector;
            switch (dto.QuestionType) {
            case "SingleOption":
                item = new model.SingleOptionQuestion();

                item.options(_.map(dto.Options, function(option) {
                    var o = new model.Option(uiId);
                    o.value(option.Value + "");
                    o.label(_.unescape(option.Label) + "");
                    if (_.isEqual(dto.Answer, option.Value)) {
                        item.selectedOption(o.value());
                        o.isSelected(true);
                    }
                    return o;
                }));

                break;
            case "MultyOption":
                item = new model.MultyOptionQuestion(_.isEmpty(dto.Settings) ? true : dto.Settings.AreAnswersOrdered, _.isEmpty(dto.Settings) ? null : dto.Settings.MaxAllowedAnswers);
                
                item.options(_.map(dto.Options, function(option) {
                    var o = new model.Option(uiId);
                    o.value(option.Value + "");
                    o.label(_.unescape(option.Label) + "");

                    return o;
                }));
                _.each(dto.Answer, function (optionValue) {
                    item.selectedOptions.push(optionValue + "");
                });
                break;
            case "Text":
                item = new model.TextQuestion();
                item.answer(_.unescape(dto.Answer));
                break;
            case "Numeric":
            case "AutoPropagate":
                item = new model.NumericQuestion(_.isEmpty(dto.Settings) ? true : dto.Settings.IsInteger, _.isEmpty(dto.Settings) ? null : dto.Settings.CountOfDecimalPlaces);

                if (_.isNumber(dto.Answer)) {
                    item.answer(dto.Answer);
                } else {
                    item.answer(parseFloat(dto.Answer));
                }
                
                break;
            case "DateTime":
                item = new model.DateTimeQuestion();
                item.answer(new Date(dto.Answer));
                break;
            case "GpsCoordinates":
                item = new model.GpsQuestion();
                if (!_.isNull(dto.Answer) && !_.isUndefined(dto.Answer)) {
                    item.latitude(dto.Answer.Latitude);
                    item.longitude(dto.Answer.Longitude);
                    item.accuracy(dto.Answer.Accuracy);
                    item.altitude(dto.Answer.Altitude);
                    item.timestamp(dto.Answer.Timestamp);
                }
                break;
            }
            var comments = _.map(dto.Comments, function(comment) {
                var c = new model.Comment();
                c.id(comment.Id);
                c.text(comment.Text);
                c.date(comment.Date);
                c.userName(comment.CommenterName);
                c.userId(comment.CommenterId);
                return c;
            });
            item.isReadonly(dto.Scope != "Supervisor");
            item.variable(dto.Variable);
            item.comments(comments);
            item.scope(dto.Scope);
            item.isAnswered(dto.IsAnswered);
            item.uiId(uiId);
            item.id(dto.Id);
            item.title(_.unescape(dto.Title));
            item.isFlagged(dto.IsFlagged);
            item.questionType(dto.QuestionType);
            item.isCapital(dto.IsCapital);
            item.isEnabled(dto.IsEnabled);
            item.isFeatured(dto.IsFeatured);
            item.isMandatory(dto.IsMandatory);
            item.rosterVector(dto.RosterVector);
            item.isInvalid(_.isBoolean(dto.IsValid) ? dto.IsValid == false : null);
            item.validationMessage(dto.ValidationMessage);
            item.validationExpression(dto.ValidationExpression);
                
            return item;
        }
    },
        group = {
            getDtoId: function(dto) { return dto.Id + "_" + dto.RosterVector; },
            fromDto: function(dto, questions) {
                var item = new model.Group();
                item.uiId(dto.Id + "_" + dto.RosterVector);
                item.id(dto.Id);
                item.depth(dto.Depth);
                var parentRosterVector = _.first(dto.RosterVector, dto.RosterVector.length - 1);
                item.parentId(dto.ParentId + "_" + parentRosterVector);
                item.rosterVector(dto.RosterVector);
                item.questions(_.map(dto.Questions, function(q) {
                    return questions.getLocalById(question.getDtoId(q));
                }));
                item.title(dto.Title);
                return item;
            }
        },
        interview = {
            getDtoId: function(dto) { return dto.Id; },
            fromDto: function(dto) {
                var item = new model.Interview();
                item.id(dto.PublicKey);
                item.title(dto.Title);
                item.status(config.statusMap[dto.Status]);
                item.questionnaireId(dto.QuestionnairePublicKey);
                return item;
            }
        },
        user = {
            getDtoId: function(dto) { return dto.Id; },
            fromDto: function(dto) {
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
};

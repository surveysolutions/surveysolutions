define('app/mapper', ['lodash', 'app/model'],
    function (_, model) {
        var question = {
            getDtoId: function (dto) { return dto.PublicKey; },
            fromDto: function (dto) {
                var item = new model.Question();
                item.id(dto.PublicKey);
                item.title(dto.Title);
                item.type(dto.QuestionType);
                item.instructions(dto.Instructions);
                item.selectedOption(dto.Answer);
                if (!_.isNull(dto.Answers)) {
                    item.options(_.map(dto.Answers, function (dtoOption) {
                        var o = option.fromDto(dtoOption);
                        if (item.isSingleOption() && o.isSelected()) {
                            item.selectedOption(o.id());
                        }
                        if (item.isMultyOption() && o.isSelected()) {
                            item.selectedOptions.push(o.id());
                        }
                        return o;
                    }));
                }
              
                return item;
            }
        },
            option = {
                getDtoId: function (dto) { return dto.PublicKey; },
                fromDto: function (dto) {

                    var item = new model.Option();
                    item.id(dto.PublicKey);
                    item.title(dto.Title);
                    item.value(dto.AnswerValue);
                    item.isSelected(dto.Selected);
                    return item;
                }
            },
        user = {
            getDtoId: function (dto) { return dto.PublicKey; },
            fromDto: function (dto) {
                var item = new model.User();
                item.id(dto.PublicKey);
                item.name(dto.UserName);
                item.email(dto.Email);
                return item;
            }
        };
        return {
            question: question,
            user: user,
            option: option
        };
    });

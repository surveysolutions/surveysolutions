﻿Mapper = function (model) {
    var question = {
        getDtoId: function (dto) { return dto.PublicKey; },
        fromDto: function (dto) {
            var item = new model.Question();
            item.id(dto.PublicKey);
            item.title(dto.Title);
            item.type(dto.QuestionType);
            item.variable(dto.StataExportCaption);
            item.instructions(dto.Instructions);
            item.selectedOption(dto.Answer);
            if (!Supervisor.Framework.Objects.isNull(dto.Answers)) {
                item.options($.map(dto.Answers, function (dtoOption) {
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
            switch (item.type()) {
                case "SingleOption":
                    item.selectedOption.extend({ required: true });
                    break;
                case "MultyOption":
                    item.selectedOptions.extend({ notempty: true });
                    break;
                case "AutoPropagate":
                case "Numeric":
                    item.selectedOption.extend({ required: true, number: true, digit: true });
                    break;
                case "DateTime":
                    item.selectedOption(new Date());
                    item.selectedOption.extend({ required: true, date: true });
                case "Text":
                    item.selectedOption.extend({ required: true });
            }
            return item;
        }
    },
        option = {
            getDtoId: function (dto) { return dto.AnswerValue; },
            fromDto: function (dto) {

                var item = new model.Option();
                item.id(dto.AnswerValue);
                item.questionId(dto.PublicKey);
                item.title(dto.Title);
                item.value(dto.AnswerValue);
                item.isSelected(dto.Selected || false);
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
};
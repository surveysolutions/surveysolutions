﻿Mapper = function (model) {
    var question = {
        getDtoId: function (dto) { return dto.Id; },
        fromDto: function (dto) {
            var item = new model.Question();
            item.id(this.getDtoId(dto));
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
                item.settings(dto.Settings);
                var isSettingsEmpty = _.isEmpty(dto.Settings);
                var isInteger = isSettingsEmpty || dto.Settings.IsInteger;
                item.selectedOption.extend({ number: true, required: true });
                if (isInteger) {
                    item.selectedOption.extend({ digit: true, required: true });
                }
                else if (!isSettingsEmpty && _.isNumber(dto.Settings.CountOfDecimalPlaces)) {
                    item.selectedOption.extend({ precision: dto.Settings.CountOfDecimalPlaces });
                }
                if (!isSettingsEmpty && _.isNumber(dto.Settings.MaxValue)) {
                    item.selectedOption.extend({ max: dto.Settings.MaxValue });
                }
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
                item.questionId(dto.Id);
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
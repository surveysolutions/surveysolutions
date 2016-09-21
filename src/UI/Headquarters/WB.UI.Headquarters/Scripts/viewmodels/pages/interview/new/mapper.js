Mapper = function (model) {
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
                    item.isFilteredCombobox = ko.observable(dto.Settings.IsFilteredCombobox || false);
                    item.selectedOption.extend({ required: true });
                    
                    if (dto.Settings.IsFilteredCombobox) {
                        item.selectedOption.extend({
                            validation:[
                            {
                                validator: function (value, params) {
                                    
                                    return true;
                                },
                                message: "Choose one of suggested values"
                            }]
                        });
                    };
                    break;
                case "MultyOption":
                    item.selectedOptions.extend({ notempty: true });
                    break;
                case "AutoPropagate":
                case "Numeric":
                    item.settings(dto.Settings);
                    var isSettingsEmpty = _.isEmpty(dto.Settings);
                    var isInteger = isSettingsEmpty || dto.Settings.IsInteger;
                    item.selectedOption.extend({ required: true });
                    if (isInteger) {
                        item.selectedOption.extend({ numericValidator: -1, numberLengthValidator: 'integer' });
                    }
                    else if (!isSettingsEmpty) {
                        if (_.isNumber(dto.Settings.CountOfDecimalPlaces))
                            item.selectedOption.extend({ numericValidator: dto.Settings.CountOfDecimalPlaces, numberLengthValidator: 'real' });
                        else
                            item.selectedOption.extend({ numericValidator: true, numberLengthValidator: 'real' });
                    }
                    if (!isSettingsEmpty && _.isNumber(dto.Settings.MaxValue)) {
                        item.selectedOption.extend({ max: dto.Settings.MaxValue });
                    }
                    break;
                case "DateTime":
                    item.settings(dto.Settings);
                    if (!item.settings().IsTimestamp) {
                        item.selectedOption(new Date());
                    }
                    item.selectedOption.extend({ required: true, date: true });
                    break;
                case "Text":
                    item.settings(dto.Settings);
                    var isTextSettingsEmpty = _.isEmpty(dto.Settings);
                    if (!isTextSettingsEmpty) {
                        if (!_.isEmpty(dto.Settings.Mask))
                            item.mask(dto.Settings.Mask);
                    }
                    item.selectedOption.extend({ required: true });
                    break;
                case "GpsCoordinates":
                    item.latitude.extend({ gps_latitude: true, required: true });
                    item.longitude.extend({ gps_longitude: true, required: true });

                    item.showMapUrl = function() {
                        return "http://maps.google.com/maps?q=" + item.latitude() + "," + item.longitude();
                    };
                    item.isMapVisible = function () {
                        return item.latitude.isValid() && item.longitude.isValid();
                    };
                    break;

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
                item.label(dto.Title);
                item.value(dto.AnswerValue);
                item.isSelected(dto.Selected || false);
                return item;
            }
        }
    return {
        question: question,
        option: option
    };
};
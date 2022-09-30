﻿(function () {
    angular.module('designerApp')
        .config([
            '$provide', function ($provide) {
                $provide.factory('optionsService', function (utilityService) {
                    let optionsService = {};

                    let regex = new RegExp(/^(.+?)(\.)+([-+]?\d+)((\.\.\.)(.+?))?\s*$/);

                    optionsService.validateOptionAsText = function (option) {
                        return regex.test((option || ""));
                    };

                    optionsService.stringifyOptions = function (options) {
                        let stringifiedOptions = "";
                        let maxLength = _.max(_.map(options, function (o) { return o.title.length; })) + 3;
                        _.each(options, function (option) {
                            if (!_.isEmpty(option)) {
                                stringifiedOptions += 
                                    (option.title || "") 
                                    + Array(maxLength + 1 - (option.title || "").length).join('.') 
                                    + (option.value === 0 ? "0" : (option.value || ""))
                                    + (option.attachmentName ? "..." + option.attachmentName : "");
                                stringifiedOptions += "\n";
                            }
                        });
                        return stringifiedOptions.trim("\n");
                    };

                    optionsService.parseOptions = function(stringifiedOptions) {
                        let optionsStringList = (stringifiedOptions || "").split("\n");
                        _.filter(optionsStringList, _.isEmpty);

                        let options = _.map(optionsStringList, function(item) {
                            let matches = item.match(regex);
                            let attachment = matches.length > 5 ? matches[6]: '';                                 
                            return {
                                id: utilityService.guid(),
                                value: matches[3] * 1,
                                title: matches[1],
                                attachmentName: attachment
                            };
                        });

                        return options;
                    };

                    return optionsService;
                });
            }
        ]);
})();

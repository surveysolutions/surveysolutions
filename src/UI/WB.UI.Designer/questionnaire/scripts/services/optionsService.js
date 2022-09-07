(function () {
    angular.module('designerApp')
        .config([
            '$provide', function ($provide) {
                $provide.factory('optionsService', function (utilityService) {
                    var optionsService = {};

                    var regex = new RegExp(/^(.+?)[\…\.\s]+([-+]?\d+)(\.\.\.(.+?))?\s*$/);

                    optionsService.validateOptionAsText = function (option) {
                        return regex.test((option || ""));
                    };

                    optionsService.stringifyOptions = function (options) {
                        var stringifiedOptions = "";
                        var maxLength = _.max(_.map(options, function (o) { return o.title.length; })) + 3;
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
                        var optionsStringList = (stringifiedOptions || "").split("\n");
                        _.filter(optionsStringList, _.isEmpty);

                        var options = _.map(optionsStringList, function(item) {
                            var matches = item.match(regex);
                            var attachment = matches.length > 3 ? matches[4]: '';                                 
                            return {
                                id: utilityService.guid(),
                                value: matches[2] * 1,
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

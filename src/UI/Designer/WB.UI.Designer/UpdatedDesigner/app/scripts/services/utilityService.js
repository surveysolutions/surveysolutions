(function () {
    angular.module('designerApp')
        .config([
            '$provide', function($provide) {
                $provide.factory('optionsService', function (utilityService) {
                    var optionsService = {};

                    var regex = new RegExp(/^([^\.]+)[\.\s]+([-+]?\d+(?:\.\d+)?)$/);

                    optionsService.validateOptionAsText = function(option) {
                        return regex.test((option || "").trim());
                    };

                    optionsService.stringifyOptions = function (options) {
                        var stringifiedOptions = "";
                        var maxLength = _.max(_.map(options, function(o) { return o.title.length })) + 3;
                        _.each(options, function (option) {
                            if (!_.isEmpty(option)) {
                                //stringifiedOptions += (option.title || "") + "..." + (option.value || "");
                                stringifiedOptions += _.padRight(option.title || "", maxLength, '.') + (option.value || "");
                                stringifiedOptions += "\n";
                            }
                        });
                        return stringifiedOptions.trim("\n");
                    };

                    optionsService.parseOptions = function (stringifiedOptions) {
                        var optionsStringList = (stringifiedOptions || "").split("\n");
                        _.remove(optionsStringList, _.isEmpty);
                      
                        var options = _.map(optionsStringList, function (item) {
                            var matches = item.match(regex);
                            return {
                                id: utilityService.guid(),
                                value: matches[2] * 1,
                                title: matches[1]
                            };
                        });

                        return options;
                    }

                    return optionsService;
                });
            }
        ]);

    angular.module('designerApp')
        .factory('utilityService', [
            '$rootScope', '$timeout',
            function ($rootScope, $timeout) {
                var utilityService = {};

                utilityService.guid = function () {
                    function s4() {
                        return Math.floor((1 + Math.random()) * 0x10000)
                            .toString(16)
                            .substring(1);
                    }

                    return s4() + s4() + s4() + s4() +
                        s4() + s4() + s4() + s4();
                };

                utilityService.format = function (format) {
                    var args = Array.prototype.slice.call(arguments, 1);
                    return format.replace(/{(\d+)}/g, function (match, number) {
                        return typeof args[number] !== 'undefined'
                            ? args[number]
                            : match;
                    });
                };

                utilityService.focus = function (name) {
                    $timeout(function () {
                        $rootScope.$broadcast('focusOn', name);
                    });
                };

                utilityService.createQuestionForDeleteConfirmationPopup = function(title) {
                    var trimmedTitle = title.substring(0, 15) + (title.length > 15 ? "..." : "");
                    var message = 'Are you sure you want to delete "' + trimmedTitle + '"?';
                    return {
                        title: message,
                        okButtonTitle: "DELETE",
                        cancelButtonTitle: "BACK TO DESIGNER"
                    };
                };

                return utilityService;
            }
        ]);
})();
angular.module('designerApp')
    .directive('denyThisKeyword', function () {
        'use strict';
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, elm, attrs, ctrl) {
                ctrl.$parsers.unshift(function (viewValue) {
                    if (!_.isEmpty(viewValue)) {
                        var thisKeywordUsed = viewValue.indexOf('[this]') > -1;
                        ctrl.$setValidity('denyThisKeyword', !thisKeywordUsed);
                    } else {
                        ctrl.$setValidity('denyThisKeyword', true);
                    }
                    return viewValue;
                });
            }
        };
    });

angular.module('designerApp')
    .directive('matchOptionsPattern', ['optionsService', function (optionsService) {
        'use strict';
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, elm, attrs, ctrl) {
                var validateOptionsString = function (viewValue) {
                    if (!_.isEmpty(viewValue)) {
                        var options = (viewValue || "").split("\n");
                        var matchPattern = true;
                        _.each(options, function (option) {
                            matchPattern = matchPattern && optionsService.validateOptionAsText(option);
                        });
                        ctrl.$setValidity('matchOptionsPattern', matchPattern);
                    } else {
                        ctrl.$setValidity('matchOptionsPattern', true);
                    }
                    return viewValue;
                };

                ctrl.$parsers.unshift(validateOptionsString);
                ctrl.$formatters.unshift(validateOptionsString);
            }
        };
    }]);

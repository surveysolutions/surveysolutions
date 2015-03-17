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
    }]).directive('maxOptionsCount', [function () {
        'use strict';
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, elm, attrs, ctrl) {
                var validateOptionsString = function (viewValue) {
                    if (!_.isEmpty(viewValue)) {
                        var options = (viewValue || "").split("\n");
                        ctrl.$setValidity('maxOptionsCount', options.length <= 200);
                    } else {
                        ctrl.$setValidity('maxOptionsCount', true);
                    }
                    return viewValue;
                };

                ctrl.$parsers.unshift(validateOptionsString);
                ctrl.$formatters.unshift(validateOptionsString);
            }
        };
    }]);

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

                    }
                });
            }
        };
    });

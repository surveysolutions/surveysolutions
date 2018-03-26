﻿(function($) {
    angular.module('designerApp').directive('focusOnOut', function () {
        return function(scope, elem, attr) {
            scope.$on('focusOn', function(e, name) {
                if (name === attr.focusOnOut) {
                    elem[0].focus();
                }
            });
            
            scope.$on('focusOut', function (e, name) {
                if (name === attr.focusOnOut) {
                    elem[0].blur();
                }
            });
        };
    }).directive('splitArray', function() {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function(scope, element, attr, ngModel) {
                function fromUser(text) {
                    return text.split("\n");
                }

                function toUser(array) {
                    if (!_.isArray(array))
                        return null;
                    return array.join("\n");
                }

                ngModel.$parsers.push(fromUser);
                ngModel.$formatters.push(toUser);
            }
        };
    }).directive("emptyTag", function () {
        return {
            restrict: "E",
            replace: true,
            template: ""
        };
    })
    .directive('onEnter', function () {

        var linkFn = function (scope, element, attrs) {
            element.bind("keypress", function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.onEnter);
                    });
                    event.preventDefault();
                }
            });
        };

        return {
            link: linkFn
        };
    }).directive('dragAndDrop', function () {

        var linkFn = function (scope, element, attrs) {
            element.bind("mousedown", function (event) {
                element.addClass('draggable').parents().on('mousemove', function (e) {
                    $('.draggable').offset({
                        top: e.pageY - element.outerHeight() / 8,
                        left: e.pageX - element.outerWidth() / 8
                    }).on('mouseup', function () {
                        element.removeClass('draggable');
                    });
                    e.preventDefault();
                });
            });

            element.bind("mouseup", function (event) {
                element.removeClass('draggable');
            });
        };

        return {
            restrict: 'AC',
            link: linkFn
        };
    });
})(jQuery);

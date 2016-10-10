(function($) {
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
    }).directive('focusWhen', function ($timeout) {
        return {
            restrict: 'A',
            link: {
                post: function postLink(scope, element, attrs) {
                    scope.$watch(attrs.focusWhen, function () {
                        if (attrs.focusWhen) {
                            if (scope.$eval(attrs.focusWhen)) {
                                $timeout(function () {
                                    if (attrs.hasOwnProperty('uiAce')) {
                                        var edit = ace.edit(element.id);
                                        edit.focus();
                                    } else {
                                        element.focus();
                                    }
                                }, 300);
                            }
                        }
                    });
                }
            }
        };
    });
})(jQuery);

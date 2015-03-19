(function() {
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
    });
})();
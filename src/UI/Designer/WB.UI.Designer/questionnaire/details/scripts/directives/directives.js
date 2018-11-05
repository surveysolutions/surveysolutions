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
                if ($(event.target).is('input')) {
                    event.stopPropagation();
                    return;
                }

                var offset = $(element).offset();
                var x = event.pageX - offset.left;
                var y = event.pageY - offset.top;

                element.addClass('draggable').parents().on('mousemove', function (e) {
                    $('.draggable').offset({
                        top: e.pageY - y,
                        left: e.pageX - x
                    }).on('mouseup', function () {
                        element.removeClass('draggable');
                    });
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
        }).directive('jqdatepicker', function () {
            return {
                restrict: 'A',
                require: 'ngModel',
                link: function (scope, element) {
                    $(element).datepicker({
                        dateFormat: 'yy-mm-dd',

                        onSelect: function (date) {
                            var ngModelName = this.attributes['ng-model'].value;

                            // if value for the specified ngModel is a property of 
                            // another object on the scope
                            if (ngModelName.indexOf(".") != -1) {
                                var objAttributes = ngModelName.split(".");
                                var lastAttribute = objAttributes.pop();
                                var partialObjString = objAttributes.join(".");
                                var partialObj = eval("scope." + partialObjString);

                                partialObj[lastAttribute] = date;
                            }
                            // if value for the specified ngModel is directly on the scope
                            else {
                                scope[ngModelName] = date;
                            }
                            scope.$apply();
                        }

                    });
                }
            };
        });
})(jQuery);

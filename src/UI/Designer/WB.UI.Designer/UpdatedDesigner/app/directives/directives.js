(function() {
    angular.module('designerApp').directive('bsHasError', [
        function() {
            return {
                restrict: "A",
                link: function(scope, element) {
                    var input = element.find('input[ng-model]');
                    if (input) {
                        scope.$watch(function() {
                            return input.hasClass('ng-invalid');
                        }, function(isInvalid) {
                            element.toggleClass('has-error', isInvalid);
                        });
                    }
                }
            };
        }
    ]);
})();
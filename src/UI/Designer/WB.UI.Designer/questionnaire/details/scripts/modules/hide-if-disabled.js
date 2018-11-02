angular.module('designerApp')
    .directive('hideIfDisabled',
        function () {
            return {
                restrict: 'E',
                scope: {
                    hideValue: '='
                },
                controller: function ($scope) {

                },
                templateUrl: 'views/hideIfDisabled.html'
            };
        }
    );

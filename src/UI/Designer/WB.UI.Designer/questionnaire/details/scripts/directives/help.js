angular.module('designerApp')
    .directive('help',
        function(helpService) {
            return {
                restrict: 'E',
                scope: {
                    key: '@',
                    placement: '@?'
                },
                link: function(scope) {
                    scope.message = helpService.getHelpMessage(scope.key);
                },
                controller: function($scope) {
                    $scope.placement = $scope.placement || 'top';
                },
                templateUrl: 'views/help-link.html',
                replace: false //otherwise we are getting conflict with angular ui popover directive
            };
        }
    );
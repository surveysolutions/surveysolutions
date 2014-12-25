angular.module('designerApp')
    .directive('help', [
        'helpService',
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
                template: '<span tooltip="{{message}}" tooltip-append-to-body="true" tooltip-placement="{{placement}}">(<a href="javascript:void(0);" tabindex="-1">?</a>)</span>',
                replace: false //otherwise we are getting conflict with angular ui popover directive
            };
        }
    ]);
angular.module('designerApp')
    .directive('help', [
        'helpService',
        function(helpService) {
            return {
                restrict: 'E',
                scope: {
                    key: '@'
                },
                link: function(scope, element, attrs) {
                    scope.message = helpService.getHelpMessage(scope.key);
                },
                template: '<span tooltip="{{message}}" tooltip-append-to-body="true">(<a href="javascript:void(0);">?</a>)</span>',
                replace: false //otherwise we are getting conflict with angular ui popover directive
            };
        }
    ]);
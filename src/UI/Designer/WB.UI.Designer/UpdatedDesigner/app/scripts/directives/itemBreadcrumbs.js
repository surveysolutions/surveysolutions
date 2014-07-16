angular.module('designerApp')
    .directive('itemBreadcrumbs', [
        function () {
            return {
                restrict: "EA",
                scope: {
                    crumbs: '='
                },
                templateUrl: 'views/itemBreadcrumbs.html',
                link: function (scope, element, attrs) {
                    element.addClass('breadcrumbs');
                }
            };
        }
    ]);
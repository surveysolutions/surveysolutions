angular.module('designerApp')
    .directive('itemBreadcrumbs', [
        function () {
            return {
                restrict: "EA",
                scope: {
                    crumbs: '='
                },
                templateUrl: 'app/views/itemBreadcrumbs.html',
                link: function (scope, element, attrs) {
                    element.addClass('breadcrumbs');
                },
                controller: [
                    '$scope', '$routeParams', 'utilityService',
                    function ($scope, $routeParams, utils) {
                        $scope.itemUrl = function (itemId) {
                            return utils.format('#/{0}/chapter/{1}/group/{2}', $routeParams.questionnaireId, $routeParams.chapterId, itemId);
                        }
                    }]
            };
        }
    ]);
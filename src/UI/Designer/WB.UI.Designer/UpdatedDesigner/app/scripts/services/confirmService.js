(function() {
    angular.module('designerApp').factory('confirmService', [
        '$modal',
        function($modal) {
            var confirmService = {};

            confirmService.open = function(item) {
                return $modal.open({
                    templateUrl: 'views/confirm.html',
                    controller: 'confirmCtrl',
                    windowClass: 'confirm-window',
                    resolve:
                    {
                        item: function() {
                            return item;
                        }
                    }
                });
            };

            return confirmService;
        }
    ]);
})();
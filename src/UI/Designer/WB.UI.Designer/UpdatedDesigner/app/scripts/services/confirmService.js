(function() {
    angular.module('designerApp').factory('confirmService', [
        '$modal',
        function($modal) {
            var confirmService = {};

            confirmService.open = function (item) {
                var scopeItem = {
                    title: item.title || "",
                    okButtonTitle: item.okButtonTitle || "DELETE",
                    cancelButtonTitle: item.cancelButtonTitle || "BACK TO DESIGNER",
                    isReadOnly: item.isReadOnly || false
                };
                return $modal.open({
                    templateUrl: 'views/confirm.html',
                    controller: 'confirmCtrl',
                    windowClass: 'confirm-window',
                    resolve:
                    {
                        item: function() {
                            return scopeItem;
                        }
                    }
                });
            };

            return confirmService;
        }
    ]);
})();
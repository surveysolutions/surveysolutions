(function() {
    angular.module('designerApp').factory('alertService', [
        '$uibModal',
        function ($uibModal) {
            var alertService = {};

            alertService.open = function (item) {
                var scopeItem = {
                    title: item.title || "",
                    okButtonTitle: item.okButtonTitle || "OK",
                    isReadOnly: item.isReadOnly || false
                };
                return $uibModal.open({
                    templateUrl: 'views/alert.html',
                    controller: 'alertCtrl',
                    windowClass: 'alert-window',
                    resolve:
                    {
                        item: function() {
                            return scopeItem;
                        }
                    }
                });
            };

            return alertService;
        }
    ]);
})();
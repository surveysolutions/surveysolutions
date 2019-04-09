(function() {
    angular.module('designerApp').factory('confirmService', [
        '$uibModal', '$i18next',
        function ($uibModal, $i18next) {
            var confirmService = {};

            confirmService.open = function (item) {
                var scopeItem = {
                    title: item.title || "",
                    okButtonTitle: item.okButtonTitle || $i18next.t("Delete"),
                    cancelButtonTitle: item.cancelButtonTitle || $i18next.t("Cancel"),
                    isReadOnly: item.isReadOnly || false
                };
                return $uibModal.open({
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
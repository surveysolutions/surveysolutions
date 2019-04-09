(function() {
    angular.module('designerApp').factory('alertService', [
        '$uibModal', '$i18next',
        function ($uibModal, $i18next) {
            var alertService = {};

            alertService.open = function (item) {
                var scopeItem = {
                    title: item.title || "",
                    okButtonTitle: item.okButtonTitle || $i18next.t("Ok"),
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
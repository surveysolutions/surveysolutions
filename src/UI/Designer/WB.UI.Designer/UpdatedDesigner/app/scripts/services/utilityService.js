(function () {
    angular.module('designerApp')
        .factory('utilityService', [
            '$rootScope', '$timeout',
            function ($rootScope, $timeout) {
                var utilityService = {};

                utilityService.guid = function () {
                    function s4() {
                        return Math.floor((1 + Math.random()) * 0x10000)
                            .toString(16)
                            .substring(1);
                    }

                    return s4() + s4() + s4() + s4() +
                        s4() + s4() + s4() + s4();
                };

                utilityService.format = function (format) {
                    var args = Array.prototype.slice.call(arguments, 1);
                    return format.replace(/{(\d+)}/g, function (match, number) {
                        return typeof args[number] !== 'undefined'
                            ? args[number]
                            : match;
                    });
                };

                utilityService.focus = function (name) {
                    $timeout(function () {
                        $rootScope.$broadcast('focusOn', name);
                    });
                };

                utilityService.createQuestionForDeleteConfirmationPopup = function(title) {
                    var trimmedTitle = title.substring(0, 15) + (title.length > 15 ? "..." : "");
                    var message = 'Are you sure you want to delete "' + trimmedTitle + '"?';
                    return {
                        title: message,
                        okButtonTitle: "DELETE",
                        cancelButtonTitle: "BACK TO DESIGNER"
                    };
                };

                return utilityService;
            }
        ]);
})();
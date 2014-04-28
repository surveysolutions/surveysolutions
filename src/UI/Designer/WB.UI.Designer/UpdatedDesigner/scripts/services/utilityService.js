angular.module('designerApp')
    .factory('utilityService', [
        function() {
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

            return utilityService;
        }
    ]);
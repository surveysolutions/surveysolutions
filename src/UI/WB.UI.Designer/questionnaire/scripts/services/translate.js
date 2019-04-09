angular.module('designerApp')
    .factory('$translate',
        function($i18next) {
            return {
                instant: function(message) {
                    return $i18next.t(message);
                }
            }
        });
angular.module('designerApp').filter('escape', function () {
    return function (input) {
        if (input) {
            return _.escape(input);
        }
    };
});
angular.module('designerApp').filter('escape', function (utilityService) {
    return function (input) {
        return utilityService.sanitize(input);
    };
});
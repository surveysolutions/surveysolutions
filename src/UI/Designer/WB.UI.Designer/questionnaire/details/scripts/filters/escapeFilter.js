angular.module('designerApp').filter('escape', function () {
    return function (input) {
        if (input) {
            var html = filterXSS(input, {
                whiteList: [],        // empty, means filter out all tags
                stripIgnoreTag: true      // filter out all HTML not in the whilelist
            });
            return html;
        }
        return input;
    };
});
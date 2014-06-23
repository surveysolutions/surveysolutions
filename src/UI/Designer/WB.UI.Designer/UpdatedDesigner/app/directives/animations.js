(function() {
    angular.module('designerApp').animation('.slide', function() {
        var NgHideClassName = 'ng-hide';
        return {
            beforeAddClass: function(element, className, done) {
                if (className === NgHideClassName) {
                    jQuery(element).slideUp(done);
                }
            },
            removeClass: function(element, className, done) {
                if (className === NgHideClassName) {
                    jQuery(element).hide().slideDown(done);
                }
            }
        }
    });
})();
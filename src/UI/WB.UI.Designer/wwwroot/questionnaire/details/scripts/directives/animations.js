angular.module('designerApp')
    .animation('.slide', function () {
        var ngHideClassName = 'ng-hide';
        return {
            beforeAddClass: function (element, className, done) {
                if (className === ngHideClassName) {
                    jQuery(element).slideUp(done);
                }
            },
            removeClass: function (element, className, done) {
                if (className === ngHideClassName) {
                    jQuery(element).hide().slideDown(done);
                }
            }
        };
    });

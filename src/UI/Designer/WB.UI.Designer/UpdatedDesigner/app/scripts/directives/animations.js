(function() {
    angular.module('designerApp')
    .animation('.slide', function () {
        var ngHideClassName = 'ng-hide';
        return {
            beforeAddClass: function(element, className, done) {
                if (className === ngHideClassName) {
                    jQuery(element).slideUp(done);
                }
            },
            removeClass: function(element, className, done) {
                if (className === ngHideClassName) {
                    jQuery(element).hide().slideDown(done);
                }
            }
        };
    })
    .animation('.filter-animate', function () {
        return {
            enter: function(element, done) {
                element.css('opacity', 0);
                jQuery(element).animate({
                    opacity: 1
                }, done);

                // optional onDone or onCancel callback
                // function to handle any post-animation
                // cleanup operations
                return function(isCancelled) {
                    if (isCancelled) {
                        jQuery(element).stop();
                    }
                };
            },
            leave: function(element, done) {
                element.css('opacity', 1);
                jQuery(element).animate({
                    opacity: 0
                }, done);

                // optional onDone or onCancel callback
                // function to handle any post-animation
                // cleanup operations
                return function(isCancelled) {
                    if (isCancelled) {
                        jQuery(element).stop();
                    }
                };
            },
            move: function(element, done) {
                element.css('opacity', 0);
                jQuery(element).animate({
                    opacity: 1
                }, done);

                // optional onDone or onCancel callback
                // function to handle any post-animation
                // cleanup operations
                return function(isCancelled) {
                    if (isCancelled) {
                        jQuery(element).stop();
                    }
                };
            }
        };
    });
})();
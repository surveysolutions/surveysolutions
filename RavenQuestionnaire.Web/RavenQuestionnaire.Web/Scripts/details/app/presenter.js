define('presenter',
    ['jquery'],
    function ($) {
        var // methods
            transitionOptions = {
                ease: 'swing',
                fadeOut: 100,
                floatIn: 500,
                offsetLeft: '20px',
                offsetRight: '-20px'
            },
            toggleActivity = function(show) {
                $('#busyindicator').activity(show);
               if (!show) {
                   $.when($('.loading-backdrop').slideUp(800))
                       .then(function() {
                           $('body').removeClass('loading');
                       });
               } else {
                    $('body').addClass('loading');
                    $('.loading-backdrop').slideDown(800);
                }
            };

        return {
            toggleActivity: toggleActivity
        };
    });

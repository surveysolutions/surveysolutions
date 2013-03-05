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
                if (show) {
                    $('body').addClass('loading');
                } else {
                    $('body').removeClass('loading');
                }
            };

        return {
            toggleActivity: toggleActivity
        };
    });

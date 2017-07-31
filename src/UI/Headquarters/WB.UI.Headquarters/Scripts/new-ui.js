jQuery.fn.preventDoubleSubmission = function () {
    $(this).on('submit', function (e) {
        var $form = $(this);

        if ($form.data('submitted') === true) {
            // Previously submitted - don't submit again
            e.preventDefault();
        } else if ($form.valid()) {
            // Mark it so that the next submit can be ignored
            $form.data('submitted', true);
        }
    });

    // Keep chainability
    return this;
};

var ajustNoticeHeight = function () {
    var height = $(".view-mode").outerHeight();
    $('.view-mode + main').css("margin-top", height + "px");
    $('.wrapper-view-mode').css("padding-top", height);
    $('.wrapper-view-mode .foldback-button').css("margin-top", height);
    $('.wrapper-view-mode .humburger-foldback-button').css("margin-top", height);
};


$(function() {
    var globalSettings = window.input.settings;
    
    $("#hide-filters").click(function () {
        $(".filters").toggleClass("hidden-filters");
        $(this).parents('.row').toggleClass("fullscreen-hidden-filters");
    });
    $("main").removeClass("hold-transition");
    $("footer").removeClass("hold-transition");

    $(window).on('resize',
        function() {
            if ($(window).width() > 880) {
                if ($(".navbar-collapse.collapse.in").length > 0) {
                    $("main").addClass("display-block");
                }
            } else {
                $("main").removeClass("display-block");
            }
        });

    $(".navbar-toggle").click(function() {
        $(".navbar-collapse").fadeToggle();
        $(".navbar-collapse").animate({ height: '100%' }, 0);
        $(".top-menu").toggleClass("top-animate");
        $(".mid-menu").toggleClass("mid-animate");
        $(".bottom-menu").toggleClass("bottom-animate");
        if ($(window).width() < 880) {
            if ($(".navbar-collapse.collapse.in").length > 0) {
                $("main").removeClass("display-block");
                $("main").removeClass("hidden");
            } else {

                $("main").addClass("hidden");
            }
        }
    });

    $('form').preventDoubleSubmission();

    var syncQueueConfig = globalSettings.config.syncQueue;
    if (syncQueueConfig.enabled) {
        var updateQueueLength = function() {
            $.ajax({
                url: syncQueueConfig.lengthUrl,
                type: 'get',
                dataType: 'json',
                success: function(data) {
                    $('#sync-queue-size').text(data);
                    if (data > 0) {
                        $('#IncomingPackagesQueueIndicator').fadeIn();
                    } else {
                        $('#IncomingPackagesQueueIndicator').fadeOut();
                    }
                }
            });
        }

        setInterval(updateQueueLength, 3000);
    }

    $(".view-mode + main .container-fluid .filters").wrapInner("<div class='wrapper-view-mode'></div>");
    $(".view-mode + main .container-fluid .content").wrapInner("<div class='wrapper-view-mode'></div>");
    ajustNoticeHeight();

    $('.view-mode .alerts .alert').on('closed.bs.alert', function() {
        ajustNoticeHeight();
    });
});

$(window).resize(function () {
    ajustNoticeHeight();
});
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

var fixAdaptiveFooterIfNeeded = function () {
    if ($(window).width() > 1300) {
        $('.row').removeClass("fullscreen-hidden-filters");
        if (!$("footer").hasClass("footer-adaptive")) {
            $("footer").addClass("footer-adaptive");
        }
    }
};

$(function() {
    var globalSettings = window.input.settings;

    if ($('#hide-filters').length > 0) {
        $("footer").addClass("footer-adaptive");
    }
    
    $("#hide-filters").click(function () {
        $(".filters").toggleClass("hidden-filters");
        $(this).parents('.row').toggleClass("fullscreen-hidden-filters");
        $("footer").toggleClass("footer-adaptive");
    });
    $("main").removeClass("hold-transition");
    $("footer").removeClass("hold-transition");
    $(".navbar-toggle").click(function() {
        $(".navbar-collapse").fadeToggle();
        $(".navbar-collapse").animate({ height: '100%' }, 0);
        $(".top-menu").toggleClass("top-animate");
        $(".mid-menu").toggleClass("mid-animate");
        $(".bottom-menu").toggleClass("bottom-animate");
        $("main").toggleClass("hidden");
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
    fixAdaptiveFooterIfNeeded();
});
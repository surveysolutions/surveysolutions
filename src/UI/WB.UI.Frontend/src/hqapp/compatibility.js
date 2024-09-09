import $ from 'jquery';
window.jQuery = $;

$.fn.preventDoubleSubmission = function () {
    $(this).on('submit', function (e) {
        var $form = $(this)

        if ($form.data('submitted') === true) {
            // Previously submitted - don't submit again
            e.preventDefault()
        } else {
            $form.data('submitted', true)
        }
    })

    // Keep chainability
    return this
}

window.ajustNoticeHeight = function () {
    var height = $('.view-mode').outerHeight()
    $('.view-mode + main .container-fluid .panel-details').css('padding-top', height)
}
window.ajustDetailsPanelHeight = function () {
    var height = $('.view-mode').outerHeight()

    var panelDetails = $('.panel-details')
    if (panelDetails.length > 0) {
        height = panelDetails.outerHeight()
    }

    $('.filters').css('top', height + 'px')
    $('.filters-results').css('top', height + 'px')
    $('.content').css('top', height + 'px')
    $('main').css('margin-top', height + 'px')
}

$(function () {
    $('main').removeClass('hold-transition')
    $('footer').removeClass('hold-transition')

    $(window).on('resize',
        function () {
            if ($(window).width() > 880) {
                if ($('.navbar-collapse.collapse.show').length > 0) {
                    $('main').addClass('display-block')
                }
            } else {
                $('main').removeClass('display-block')
            }
        })

    $('.navbar-toggle').click(function () {
        $('.navbar-collapse').fadeToggle()
        $('.navbar-collapse').animate({ height: '100%' }, 0)
        $('.top-menu').toggleClass('top-animate')
        $('.mid-menu').toggleClass('mid-animate')
        $('.bottom-menu').toggleClass('bottom-animate')
        if ($(window).width() < 880) {
            if ($('.navbar-collapse.collapse.show').length > 0) {
                $('main').removeClass('display-block')
                $('main').removeClass('hidden')
            } else {

                $('main').addClass('hidden')
            }
        }
    })

    $('form').preventDoubleSubmission()


    window.ajustNoticeHeight()
    window.ajustDetailsPanelHeight()

    $('.view-mode .alerts .alert').on('closed.bs.alert', function () {
        window.ajustNoticeHeight()
        window.ajustDetailsPanelHeight()
    })
})

$(window).resize(function () {
    window.ajustNoticeHeight()
    window.ajustDetailsPanelHeight()
})

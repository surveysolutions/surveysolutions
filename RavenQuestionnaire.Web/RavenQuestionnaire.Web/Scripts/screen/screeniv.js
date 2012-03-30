/*bad behaviour on touch device and if height is the same as window*/
/*<script type="text/javascript">
!function ($) {

$(function () {
// fix sub nav on scroll
var $win = $(document), $nav = $('.subnav'), navTop = $('.subnav').length && $('.subnav').offset().top - 40, isFixed = 0;

processScroll();

$win.on('scroll', processScroll);

function processScroll() {
var i, scrollTop = $win.scrollTop();
if (scrollTop >= navTop && !isFixed) {
isFixed = 1
$nav.addClass('subnav-fixed');
                    
} else if (scrollTop <= navTop && isFixed) {
isFixed = 0
$nav.removeClass('subnav-fixed');
}
}
});
} (window.jQuery);
</script>*/

$(document).ready(function () {
    /* Horizontal scroll for propagated groups */
//    if (typeof document.body.style.webkitOverflowScrolling === "undefined") {
//        var xScrollers = document.getElementsByClassName("scroll-x");
//        for (var i = 0; i < xScrollers.length; i++)
//            new iScroll(xScrollers[i], { vScroll: false });
//    }


});
$(document).ready(function() {
    /* Scroll fix for inputs */
//    $('input[type="text"]').live('focus', function(e) {
//        $(this).focus(); e.stopPropagation();
//    });
//    $('input[type="number"]').live('click', function(e) {
//        $(this).focus(); e.stopPropagation();
//    });
});
$(document).ready(function () {
    adjust();
});
$(window).resize(function () {
    adjust();
});
function adjust() {
    $('#headerTableWrap').css("width", $('#contentTableWrap').css('width'));
}

jQuery(document).ready(function ($) {

    $(".gallery").each(function (i) {
        var id = $(this).attr('id').replace('images', '');
        // We only want these styles applied when javascript is enabled
        $('div.content').css('display', 'block');

        // Initially set opacity on thumbs and add
        // additional styling for hover effect on thumbs
        var onMouseOutOpacity = 0.5;
        $('#thumbs' + id + ' ul.thumbs li, div.navigation a.pageLink').opacityrollover({
            mouseOutOpacity: onMouseOutOpacity,
            mouseOverOpacity: 1.0,
            fadeSpeed: 'fast',
            exemptionSelector: '.selected'
        });

        // Initialize Advanced Galleriffic Gallery
        var gallery = $('#thumbs' + id).galleriffic({
            delay: 2500,
            numThumbs: 10,
            preloadAhead: 10,
            enableTopPager: false,
            enableBottomPager: false,
            imageContainerSel: '#slideshow' + id,
            controlsContainerSel: '#controls' + id,
            captionContainerSel: '#caption' + id,
            loadingContainerSel: '#loading' + id,
            renderSSControls: true,
            renderNavControls: true,
            prevLinkText: 'Previous Card',
            nextLinkText: 'Next Card',
            nextPageLinkText: 'Next &rsaquo;',
            prevPageLinkText: '&lsaquo; Prev',
            autoStart: false,
            syncTransitions: true,
            defaultTransitionDuration: 100,
            onSlideChange: function (prevIndex, nextIndex) {
                // 'this' refers to the gallery, which is an extension of $('#thumbs')
                this.find('ul.thumbs').children()
                        .eq(prevIndex).fadeTo('fast', onMouseOutOpacity).end()
                        .eq(nextIndex).fadeTo('fast', 1.0);
            },
            onPageTransitionOut: function (callback) {
                this.fadeTo('fast', 0.0, callback);
            },
            onPageTransitionIn: function () {
                this.fadeTo('fast', 1.0);
            }
        });
    });
});
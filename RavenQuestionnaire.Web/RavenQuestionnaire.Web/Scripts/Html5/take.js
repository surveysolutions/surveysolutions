
function ReInitMobile(id) {
    //  alert('test');
    $(id).trigger('create');
    initCardGalleries();
//    ReinitInputs();
}

$('div[data-role=page]').live('pageshow', function(event) {
    resizeContent();
});

//function ReinitInputs() {
//    $("input[input-label=True]").each(function () {
//        var div = $(this).parent();
//        //  div.parent().css('position','relative');
//       /* div.css('margin', '0');
//        div.css('position', 'absolute');
//        div.css('z-index', '100');
//        div.css('top', div.prev().offset().top + 3 + 'px');
//        div.css('width', '25%');
//        div.css('right', div.width() / 2 + 'px');*/
//    });
//}

function resizeContent() {
    var newSize = $(window).height() - 100;
    var content = $('.content-primary');
    content.css("height", newSize + "px");
    content.css("overflow-y", "auto");
}

$(document).ready(function() {
    jQuery.extend(jQuery.mobile.datebox.prototype.options, {
        'dateFormat': 'MM/dd/YYYY',
        'headerFormat': 'MM/dd/YYYY'
    });
    resizeContent();
    $(window).resize(function() {
        resizeContent();
    });
});
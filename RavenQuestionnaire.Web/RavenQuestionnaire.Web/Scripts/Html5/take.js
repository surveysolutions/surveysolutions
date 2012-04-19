
function ReInitMobile(id) {
    //  alert('test');
    $(id).trigger('create');
//    ReinitInputs();
 createKeyBoard();
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

function createKeyBoard() {
    var k = $('input[type=text]');

    var kbOptions = {
        keyBinding: 'mousedown touchstart',
        position: {
            of: 'center bottom', // optional - null (attach to input/textarea) or a jQuery object (attach elsewhere)
            my: 'center top',
            at: 'center top',
            at2: 'center bottom' // used when "usePreview" is false (centers the keyboard at the bottom of the input/textarea)
        },
        // make sure jQuery UI styles aren't applied even if the stylesheet has loaded
        // the Mobile UI theme will still over-ride the jQuery UI theme
        css: {
            input: '',
            container: '',
            buttonDefault: '',
            buttonHover: '',
            buttonActive: '',
            buttonDisabled: ''
        }
    };

    k.keyboard(kbOptions).addMobile({
        // keyboard wrapper theme
        container: { theme: 'c' },
        // theme added to all regular buttons
        buttonMarkup: { theme: 'c', shadow: 'true', corners: 'true' },
        // theme added to all buttons when they are being hovered
        buttonHover: { theme: 'c' },
        // theme added to action buttons (e.g. tab, shift, accept, cancel);
        // parameters here will override the settings in the buttonMarkup
        buttonAction: { theme: 'b' },
        // theme added to button when it is active (e.g. shift is down)
        // All extra parameters will be ignored
        buttonActive: { theme: 'e' }
    });
    k.bind('accepted.keyboard', function (event) {
        $(this.form).submit();
    });
}
$(document).ready(function () {
    createKeyBoard();
    jQuery.extend(jQuery.mobile.datebox.prototype.options, {
        'dateFormat': 'MM/dd/YYYY',
        'headerFormat': 'MM/dd/YYYY'
    });
    resizeContent();
    $(window).resize(function () {
        resizeContent();
    });
   

});
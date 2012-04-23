
function ReInitMobile(id) {
    //  alert('test');
    $(id).trigger('create');
//    ReinitInputs();
    createKeyBoard();

}

function JsonResults (data, status, xhr) {

    var group = jQuery.parseJSON(data.responseText);
    UpdateGroup(group);
    }

function UpdateGroup(group) {
    for (var i = 0; i < group.Questions.length; i++) {
        UpdateQuestion(group.Questions[i]);
    }
    for (var p = 0; p < group.PropagatedGroups.length; p++) {
        for (var qp = 0; qp < group.PropagatedGroups[p].Questions.length; qp++) {
            UpdateQuestion(group.PropagatedGroups[p].Questions[qp], group.PropagatedGroups[p].PropogationKey);
        }
    }
    for (var j = 0; j < group.Groups.length; j++) {
        UpdateGroup(group.Groups[j]);
    }
}
function UpdateQuestion(question, propagationKey) {
    var questionElement = propagationKey ? $('#propagatedGroup' + propagationKey + ' #question' + question.PublicKey) : $('#question' + question.PublicKey);

    var bodyClass = question.Valid ? question.Enabled ? "" : "ui-disabled" : "ui-body ui-body-e";
    questionElement.attr("class", bodyClass);
}

function RemovePropagatedGroup(data, status, xhr) {
    var group = jQuery.parseJSON(data.responseText);
    $('#propagatedGroup' + group.propagationKey).remove();
    
}

function PropagatedGroup(data, status, xhr) {
    var group = jQuery.parseJSON(data.responseText);
    var templateDivPath = '#groupTemplate' + group.parentGroupPublicKey;
    var parent = $(templateDivPath).parent();
    var template = $(templateDivPath).html();
    var str = template.replace(/00000000-0000-0000-0000-000000000000/gi, group.propagationKey);
    var newGroup = $(str);
    var container = parent.find(" > ul:last");
    if (container.length == 0) {
        parent.prepend(newGroup);
    } else {
        newGroup.insertAfter(container);    
    }
    newGroup.createKeyBoard();
    newGroup.trigger('create');
  //  $('#foo').trigger('updatelayout');
  //  createKeyBoard();
}
/*
$(document).on('mobileinit', function () {
    $.mobile.ignoreContentEnabled = true;
});*/
//function ReInitMobileTemplate(target,id,data) {
//    $(target).html($(id).render(data));
//}

//$('div[data-role=page]').live('pageshow', function(event) {
//    //resizeContent();
//    
//});

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

/*function resizeContent() {
    var newSize = $(window).height() - 100;
    var content = $('.content-primary');
    content.css("height", newSize + "px");
    content.css("overflow-y", "auto");
}*/
(function($) {
    //jquery extension method to handle exceptions and log them

    $.fn.createKeyBoard = function() {
        var k = this.find('input[type=text]');

        var kbOptions = {
            keyBinding: 'mousedown touchstart',
            position: {
                of: null, // optional - null (attach to input/textarea) or a jQuery object (attach elsewhere)
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
            buttonMarkup: { theme: 'c', shadow: 'true', corners: 'false' },
            // theme added to all buttons when they are being hovered
            buttonHover: { theme: 'c' },
            // theme added to action buttons (e.g. tab, shift, accept, cancel);
            // parameters here will override the settings in the buttonMarkup
            buttonAction: { theme: 'b' },
            // theme added to button when it is active (e.g. shift is down)
            // All extra parameters will be ignored
            buttonActive: { theme: 'e' }
        });
        k.bind('accepted.keyboard', function(event) {
            $(this.form).submit();
        });
        k.bind('canceled.keyboard', function(event) {
            $(this).getkeyboard().accept();
        });
        k.bind('visible.keyboard', function(event) {
            // $(this).getkeyboard().css
            var keyboard = $(this).getkeyboard().$keyboard;
            keyboard.css('width', '892px');
            keyboard.css('left', '0px');
        });
    };

})(jQuery);

     


function initDateTime() {
    jQuery.extend(jQuery.mobile.datebox.prototype.options, {
        'dateFormat': 'MM/dd/YYYY',
        'headerFormat': 'MM/dd/YYYY'
    });
}
$(document).ready(function () {

    initDateTime();
    $(document).createKeyBoard();
    /*   resizeContent();
    $(window).resize(function () {
    resizeContent();
    });*/


});
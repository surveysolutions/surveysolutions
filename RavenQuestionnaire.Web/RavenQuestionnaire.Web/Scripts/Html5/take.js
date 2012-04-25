

function JsonResults (data, status, xhr) {

    var group = jQuery.parseJSON(data.responseText);
    if (!group.error)
        UpdateGroup(group);
    else
        SetErrorToQuestion(group.question,group.settings.PropogationPublicKey,group.error);
    
}

function SetErrorToQuestion(question, key,error) {
     var questionElement = key? $('#propagatedGroup' + key + ' #question' + question.PublicKey) : $('#question' + question.PublicKey);
    questionElement.find('[data-valmsg-replace=true]').text(error);
}
function UpdateGroup(group) {
    if(group.Questions) {
        for (var i = 0; i < group.Questions.length; i++) {
            UpdateQuestion(group.Questions[i]);
        }
    }
    if (group.PropagatedGroups) {
        for (var p = 0; p < group.PropagatedGroups.length; p++) {
            if (group.PropagatedGroups[p].Questions) {
                for (var qp = 0; qp < group.PropagatedGroups[p].Questions.length; qp++) {
                    UpdateQuestion(group.PropagatedGroups[p].Questions[qp], group.PropagatedGroups[p].PropogationKey);
                }
            }
        }
    }
    if (group.Groups) {
        for (var j = 0; j < group.Groups.length; j++) {
            UpdateGroup(group.Groups[j]);
        }
    }
}
function UpdateQuestion(question, propagationKey) {
    var questionElement = propagationKey ? $('#propagatedGroup' + propagationKey + ' #question' + question.PublicKey) : $('#question' + question.PublicKey);

    var bodyClass = question.Valid ? question.Enabled ? "" : "ui-disabled" : "ui-body error_block";
    questionElement.attr("class", bodyClass);
    if (!question.Enabled)
        questionElement.closest("form").clear_form_elements();
    SetErrorToQuestion(question, propagationKey, '');
}

function RemovePropagatedGroup(data, status, xhr) {
    var group = jQuery.parseJSON(data.responseText);
    $('#propagatedGroup' + group.propagationKey).remove();
    
}

function PropagatedGroup(data, status, xhr) {
    var group = jQuery.parseJSON(data.responseText);
    var templateDivPath = '#groupTemplate' + group.parentGroupPublicKey;
    var parent = $(templateDivPath).parent();
    var validator = parent.find('[data-valmsg-replace=true]');
    if (group.error) {
        validator.text(group.error);
        return; 
    }
    validator.text('');
    var template = $(templateDivPath).html();
    var str = template.replace(/00000000-0000-0000-0000-000000000000/gi, group.propagationKey);
    var newGroup = $(str);
    var container = parent.find(" > ul:last");
    if (container.length == 0) {
        parent.prepend(newGroup);
    } else {
        newGroup.insertAfter(container);    
    }
    newGroup.listview();
    newGroup.trigger('pagecreate');
   // newGroup.parent().listview('refresh');
  //  newGroup.page();
    newGroup.createKeyBoard();
    newGroup.numericSubmit();
    newGroup.hideInputsWithVirtualKeyboard();
 //  
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
    $.extend($.keyboard.layouts,{'qwertyNoEnter' : {
			'default': [
				'` 1 2 3 4 5 6 7 8 9 0 - = {bksp}',
				'{tab} q w e r t y u i o p [ ] \\',
				'a s d f g h j k l ; \'',
				'{shift} z x c v b n m , . / {shift}',
				'{accept} {space} {cancel}'
			],
			'shift': [
				'~ ! @ # $ % ^ & * ( ) _ + {bksp}',
				'{tab} Q W E R T Y U I O P { } |',
				'A S D F G H J K L : " {enter}',
				'{shift} Z X C V B N M < > ? {shift}',
				' {space} {cancel}'
			]
		}});

    $.fn.clear_form_elements=function() {

    $(this).find(':input').each(function() {
        switch(this.type) {
            case 'password':
            case 'select-multiple':
            case 'select-one':
            case 'text':
            case 'textarea':
                $(this).val('');
                break;
            case 'checkbox':
            case 'radio':
                this.checked = false;
                $(this).checkboxradio("refresh");
        }
    });

},
    //jquery extension method to handle exceptions and log them
    $.fn.numericSubmit =function()
    {
        var input = this.find('input[type=number], input[type=range]');
        var target = input.parent();
        target.find('.ui-slider a').bind('vmouseup', function() {  $($(this).parent().siblings('input')[0].form).submit(); });
      //  this.createKeyBoard('num');
    },
    $.fn.hideInputsWithVirtualKeyboard = function () {
        var virtualIcons = this.find('a[open-virtual-keyboar=true]');
        virtualIcons.each(function() {
            var button = $(this);
            var target = button.attr('target-input');
            var targetInput = $('#' + target);
          //  targetInput.createKeyBoard();
            var label = $('[for=' + target+']');
            if($.client.os=='Windows' || targetInput.attr('data-role')=='datebox') {
                targetInput.css('display', 'none');
            } else {
                button.css('display', 'none');
                label.css('display', 'none');
            }

            targetInput.change(function() {
                label.html(targetInput.val());
            });
            button.bind('click', function() {
                targetInput.focus();
                 targetInput.click();
            });


        });
    },
    $.fn.createKeyBoard = function(layout) {
        //layout = typeof layout !== 'undefined' ? layout : 'qwertyNoEnter';
        var k = this.find('input[draw-key-board=true]');
        k.removeAttr("draw-key-board");
        if($.client.os!='Windows') {
            k.each(function() {
                var input = this;
                $(input.form).bind('submit', function() {
                    input.blur();
                });
            });
            return;
        }
        var kbOptions = {
            keyBinding: 'mousedown touchstart',
            // layout : 'num',
            position: {
                of: null, // optional - null (attach to input/textarea) or a jQuery object (attach elsewhere)
                my: 'center top',
                at: 'center top',
                at2: 'center bottom' // used when "usePreview" is false (centers the keyboard at the bottom of the input/textarea)
            },
            autoAccept   : true,
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

        k.each(function () {
            var jInput = $(this);
            var options = $.extend(kbOptions, { layout: jInput.attr('type')=='text' ? 'qwertyNoEnter': 'num' });
            jInput.keyboard(options);
        
        }).addMobile({
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
            $(this).change();
            $(this.form).submit();
        });
        k.bind('visible.keyboard', function(event) {
            // $(this).getkeyboard().css
            
            var keyboard = $(this).getkeyboard().$keyboard;
            var input =keyboard.find('input');
            if($(this).attr('type')=='text'){
                keyboard.css('width', '892px');
            keyboard.css('left', '0px');}
            input.caretToEnd();
        });
    };

})(jQuery);

$(document).ready(function () {

    
    var doc = $(document);
    doc.createKeyBoard();
    doc.numericSubmit();
    doc.hideInputsWithVirtualKeyboard();

});
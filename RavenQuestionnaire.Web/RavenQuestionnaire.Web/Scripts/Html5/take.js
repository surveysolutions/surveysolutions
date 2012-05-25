
function sichronizationStarted(data, status, xhr) {
    $('a#btnSync span span').html('Processing');

    $('[data-id=main]').addClass('ui-disabled');
}
function sinchronizationCompleted(data, status, xhr) {
    $('a#btnSync span span').html('Synchronize');
    $('a#btnSync').removeClass('ui-btn-active');
    $('[data-id=main]').removeClass('ui-disabled');
    if (data.responseText != 'True')
        alert('synchronization wasn\'t succefull');
}

function JsonResults (data, status, xhr) {

    var group = jQuery.parseJSON(data.responseText);
    if (!group.error) {
        UpdateGroup(group);
        $("#counter-" + group.PublicKey).html(group.Totals.Answered + "/" + group.Totals.Enablad);
    }
    else
        SetErrorToQuestion(group.question, group.settings.PropogationPublicKey, group.error);
    
}

function SetErrorToQuestion(question, key,error) {
     var questionElement = key? $('#propagatedGroup' + key + ' #question' + question.PublicKey) : $('#question' + question.PublicKey);
    questionElement.find('[data-valmsg-replace=true]').text(error);
}
function UpdateGroup(group) {

    if (group.PropagatedGroups && group.PropagatedGroups.length>0) {
        for (var p = 0; p < group.PropagatedGroups.length; p++) {
            if (group.PropagatedGroups[p].Questions) {
                for (var qp = 0; qp < group.PropagatedGroups[p].Questions.length; qp++) {
                    UpdateQuestion(group.PropagatedGroups[p].Questions[qp], group.PropagatedGroups[p].PropogationKey);
                }
            }
        }
    }else {
        if (group.Questions) {
            for (var i = 0; i < group.Questions.length; i++) {
                UpdateQuestion(group.Questions[i]);
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

    questionElement.removeClass("ui-disabled");
    if (!question.Enabled)
        questionElement.addClass("ui-disabled");

    questionElement.removeClass("ui-body"); 
    questionElement.removeClass("error_block"); 
    if (!question.Valid) {
        questionElement.addClass("ui-body");
        questionElement.addClass("error_block");
    }

    if (question.Answered) {
        questionElement.addClass("answered");  
    }

    if (!question.Enabled)
        questionElement.closest("form").clear_form_elements();
    SetErrorToQuestion(question, propagationKey, '');
}

function RemovePropagatedGroup(data, status, xhr) {
    var group = jQuery.parseJSON(data.responseText);
    
    var deleteScreen = '#screen-' + group.propagationKey;

    var prevScreen = $(deleteScreen + ' .previous-screen').attr('href');
    var nextScreen = $(deleteScreen + ' .next-screen').attr('href');

    if (!(nextScreen == undefined || nextScreen == '' || nextScreen == '#')) {
        var nextScreenPrevLink = $(nextScreen + ' .previous-screen');
        if (nextScreenPrevLink.length > 0) {
            nextScreenPrevLink.attr('href', prevScreen);
            if (prevScreen=='#')
                $(nextScreenPrevLink).addClass('ui-disabled');
        }
    }
    if (!(prevScreen == undefined || prevScreen == '' || prevScreen == '#')) {
        var prevScreenNextLink = $(prevScreen + ' .next-screen');
        if (prevScreenNextLink.length > 0) {
            prevScreenNextLink.attr('href', nextScreen);
            if (nextScreen == '#')
                $(prevScreenNextLink).addClass('ui-disabled');
        }
    }
    var li = $('#propagatedGroup' + group.propagationKey);
    var parent = li.parent();
    $(li).remove();
    $(deleteScreen).remove();
    $(parent).listview('refresh');
    updateCounter();
}

function PropagatedGroup(data, status, xhr) {
    var group = jQuery.parseJSON(data.responseText);
    var templateDivPath = '#groupTemplate' + group.parentGroupPublicKey;
    var screenTemplateDiv = '#template-' + group.parentGroupPublicKey;
    var parent = $('#propagate-list-' + group.parentGroupPublicKey);
    
    var validator = parent.find('[data-valmsg-replace=true]');
    if (group.error) {
        validator.text(group.error);
        return; 
    }
    validator.text('');
    var template = $(templateDivPath).html();
    var screenTemplate = $(screenTemplateDiv).html();
    var str = template.replace(/00000000-0000-0000-0000-000000000000/gi, group.propagationKey);
    var screenStr = screenTemplate.replace(/00000000-0000-0000-0000-000000000000/gi, group.propagationKey);

    var screenLinks = $('.propagated-screen-link-' + group.parentGroupPublicKey);
    var lastScreen = screenLinks.last().length == 0 ? '' : screenLinks.last().attr('href');
    
    var newGroup = $(str);
    var newScreen = $.tmpl(screenStr, { PrevScreen: lastScreen, NextScreen: "#", Key: group.propagationKey }).appendTo($(screenTemplateDiv).parent());

    var prevScreenNextLink = $(lastScreen + ' .next-screen');
    $(prevScreenNextLink).attr('href', '#screen-' + group.propagationKey);
    $(prevScreenNextLink).removeClass('ui-disabled');

    
    
    var container = parent.find(" > li:last");
   
    if (container.length == 0) {
        parent.prepend(newGroup);
    } else {
        newGroup.insertAfter(container);
    }


    
    //newGroup.listview();
    newGroup.trigger('pagecreate');
    $(parent).listview('refresh');
   
    
    newScreen.page();
    newScreen.trigger('pagecreate');
    newScreen.createKeyBoard();
    newScreen.numericSubmit();
    newScreen.hideInputsWithVirtualKeyboard();

    updateCounter();
}
function updateCounter() {
    var all = $('#main').parent().find('.question').length;
    var disabled = $('#main').parent().find('.question.ui-disabled').length;
    var total = all - disabled;
    var answered = $('#main').parent().find('.question.answered').length;
    $('.ui-li-count.current').html(answered + "/" + total);
}
(function($) {
    $.extend($.keyboard.layouts,{'qwertyNoEnter' : {
			'default': [
				'` 1 2 3 4 5 6 7 8 9 0 - = ',
				'q w e r t y u i o p [ ] \\',
				'a s d f g h j k l ; \'',
				'z x c v b n m , . / {bksp}',
				'{shift} {space} {accept}'
			],
			'shift': [
				'~ ! @ # $ % ^ & * ( ) _ +',
				'Q W E R T Y U I O P { } |',
				'A S D F G H J K L : "',
				'Z X C V B N M < > ? {bksp}',
				'{shift} {space} {accept}'
			]
		},
        
        'numOnly' : {
			'default' : [
				'{dec} {sign} {b}',
				'7 8 9',
				'4 5 6',
				'1 2 3',
				'{a} 0 {c}'
			]
		}
   
    });
         $.fn.disableAfterSubmit =function() {
             var anchors = this.find('a[disable-after-click=true]');
             anchors.click(function() {
                 var button = $(this);
                  setTimeout(function() {
                     button.attr('href', '#');
                 }, 0);
             });
         },
    $.fn.clear_form_elements=function() {

    $(this).find(':input').each(function() {
        var jThis = $(this);
        switch(this.type) {
            case 'password':
            case 'select-multiple':
            case 'select-one':
            case 'text':
                 jThis.val('');
            case 'number':
                 jThis.val('');
            case 'textarea':
                jThis.val('');
                break;
            case 'checkbox':
            case 'radio':
                this.checked = false;
                jThis.checkboxradio("refresh");
        }
    });

},
    //jquery extension method to handle exceptions and log them
    $.fn.numericSubmit =function()
    {
        var input = this.find('input[type=number], input[type=range]');
        var target = input.parent();
        target.find('.ui-slider a').bind('vmouseup', function() {  $($(this).parent().siblings('input')[0].form).submit(); });
    },
    $.fn.hideInputsWithVirtualKeyboard = function () {
        var virtualIcons = this.find('a[open-virtual-keyboar=true]');
        virtualIcons.each(function() {
            var button = $(this);
            var target = button.attr('target-input');
            var targetInput = $('#' + target);
            var label = $('[from=' + target+']');
                targetInput.css('display', 'none');
       
            targetInput.change(function() {
                label.html(targetInput.val());
            });
           
          /*  var openKeyboard = function() {
                targetInput.focus();
                targetInput.click();
            };*/
      //      button.click(openKeyboard);
            button.parent().bind('tap',function() {
                targetInput.focus();
                targetInput.click();
            });

        });
    },
    $.fn.createKeyBoard = function(layout) {
        var k = this.find('input[draw-key-board=true]');
        k.removeAttr("draw-key-board");
        /*
        var key = $('.ui-keyboard-button');
        
        key.live('click',function() {
            var name = $(this).attr('data-value');
            var input = $(this).parents('.ui-keyboard').find('.ui-keyboard-preview');
            var old = $(input).attr('val-backup');
            old = (old == undefined ? '' : old);
            if ($(this).attr('name')=='dec') {
                old = old + '.';
                $(input).attr('val-backup', old);
            }else if ($(this).attr('name')=='sign') {
                if (old[0]!='-')
                    old = '-' + old;
                else {
                    old = old.substr(1);
                }
                $(input).attr('val-backup', old);
            }else if ($(this).attr('name')=='bksp') {
                old = old.substr(0, old.length - 1);
                $(input).val(old);
                $(input).attr('val-backup', old);
            } else if (isNumber(name))
            {
                
                $(input).attr('val-backup', old + name);
                //old = $(input).val();
                if (old.length > 0 && old[old.length-1]=='.') {
                    old = old + name;    
                    $(input).val(old);
                }
            }
        });
        */
       /* if($.client.os!='Windows') {
            k.each(function() {
                var input = this;
                $(input.form).bind('submit', function() {
                    input.blur();
                });
            });
            return;
        }*/
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
            var additionalOptions = { };
            if(jInput.attr('type')=='text') {
                additionalOptions = { layout: 'qwertyNoEnter', min_width: '888px' };
            }
            else {
                additionalOptions = { layout: 'numOnly', min_width: null};
            }
            var options = $.extend(kbOptions, additionalOptions);
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
             var input =$(this);
            var keyboard = input.getkeyboard();   
            keyboard.$preview.caretToEnd();
        });
    };

})(jQuery);

$(document).ready(function () {


    var doc = $(document);
    doc.createKeyBoard();
    doc.numericSubmit();
    doc.hideInputsWithVirtualKeyboard();
    doc.disableAfterSubmit();

    $('.splited-button.ui-li-link-alt').each(function () {
        var text = $(this).attr('title');
        $(this).find('.ui-btn-text').html(text);
    });

});
function isNumber(n) {
  return !isNaN(parseFloat(n)) && isFinite(n);
}

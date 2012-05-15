

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
  //  $($('div:jqmData(role="content")')[1]).iscroll().refresh();
   // $(window.document).trigger("mobileinit");
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
          /*   var inputs = this.find('input[type=submit]');
             setTimeout(function() {
                 inputs.attr('disabled', 'disabled');
             }, 1);*/

            /* this.on('submit', 'form', function() {
                 var button = $(this).find('input[type="submit"]');
                 setTimeout(function() {
                     button.attr('disabled', 'disabled');
                 }, 0);
             });*/
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
                 jThis.change();
            case 'number':
                 jThis.val('');
                 jThis.change();
            case 'textarea':
                jThis.val('');
                jThis.change();
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
      //  this.createKeyBoard('num');
    },
    $.fn.hideInputsWithVirtualKeyboard = function () {
        var virtualIcons = this.find('a[open-virtual-keyboar=true]');
        virtualIcons.each(function() {
            var button = $(this);
            var target = button.attr('target-input');
            var targetInput = $('#' + target);
          //  targetInput.createKeyBoard();
            var label = $('[from=' + target+']');
           // if($.client.os=='Windows' || targetInput.attr('data-role')=='datebox') {
                targetInput.css('display', 'none');
          /*  } else {
                button.css('display', 'none');
                label.css('display', 'none');
            }
            */
            targetInput.change(function() {
                label.html(targetInput.val());
            });
           
          /*  var openKeyboard = function() {
                targetInput.focus();
                targetInput.click();
            };*/
      //      button.click(openKeyboard);
            button.parent().click(function() {
                targetInput.focus();
               // targetInput.click();
            });

        });
    },
    $.fn.createKeyBoard = function(layout) {
        //layout = typeof layout !== 'undefined' ? layout : 'qwertyNoEnter';
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
            // $(this).getkeyboard().css
             var input =$(this);
            var keyboard = input.getkeyboard();   
            /*if(keyboard.options.min_width) {
                keyboard.$keyboard.css('width', keyboard.options.min_width);
                keyboard.$keyboard.css('left', '0px');
            }**/
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

function CheckIsCanChangePage(link) {
    var url = $(link).attr('screen-url');
    var panel = $(link).attr('data-panel');
    var pageContainer = $('[data-id=' + panel+']');
    //var fromPage = $(pageContainer.children()[0]);
    $.mobile.changePage(url, {
        pageContainer: pageContainer
    });
    $.mobile.pageContainer = pageContainer.find('.ui-page-active');
   
}

//var isokToRedirect = false;
function GetCheck(i, j) {
    $.get(i, function (data) {
        try {
            if (data.message != null && data.message == "Error") {
                $.mobile.hidePageLoadingMsg();
                $('#popup').click();
            }
            else {
                window.location = j;
            }
        } catch (e) {
            window.location = j;
        }
    });
}

function JsonResults(data, status, xhr) {

    var group = jQuery.parseJSON(data.responseText);
    if (!group.error) {
        UpdateInnerGroup(group);
        UpdateCurrentGroup(group);
        var allListElements = $('ul.ui-listview').find('li');
        allListElements.each(function () {
            var el = null;
            for (var i = 0; i < group.Menu.length; i++) {
                if (group.Menu[i].PublicKey == this.id) {
                    el = group.Menu[i];
                    break;
                }
            }
            if (el != null) {
                if (el.Enabled && $(this).is('.ui-disabled')) {
                    $(this).removeClass('ui-disabled');
                }
                else {
                    if (!el.Enabled && !($(this).is('.ui-disabled')))
                    $(this).addClass('ui-disabled');
                }
            }
        });
    }
    else
        SetErrorToQuestion(group.question, group.settings.PropogationPublicKey, group.error);

}

function UpdateComments(data) {

    var group = jQuery.parseJSON(data.responseText);
    if (!group.error) {
        UpdateCommentInGroup(group);
    }
    else
        SetErrorToQuestion(group.question, group.settings.PropogationPublicKey, group.error);
}

function UpdateCommentInGroup(group) {
    for (var j = 0; j < group.Questions.length; j++) {
        var key = group.Questions[j].PublicKey;
        var id = "#comments" + key;
        var commentscontent = group.Questions[j].Comments;
        if (commentscontent != null) {
            $(id).html('Comments: ' + commentscontent);
        }
        else {
            $(id).html('');
        }
    }
}

function UpdateInnerGroup(group) {
    for (var i = 0; i < group.InnerGroups.length; i++) {
        var groupElement = $("#"+group.InnerGroups[i].PublicKey);
        groupElement.removeClass("ui-disabled");
        if (!group.InnerGroups[i].Enabled) {
            groupElement.addClass("ui-disabled");
        }
    }
}

function UpdateCurrentGroup(group) {
    for (var j = 0; j < group.Menu.length; j++) {
        var total = group.Menu[j].Totals;
        var counterElement = $("#counter-" + group.Menu[j].PublicKey);
        counterElement.html(total.Answered + "/" + total.Enablad);
        if (total.Answered == total.Enablad)
            counterElement.addClass('complete');
        else
            counterElement.removeClass('complete');

    }
    for (var j = 0; j < group.Questions.length; j++) {
        UpdateQuestion(group.Questions[j], group.Questions[j].GroupPublicKey);
    }
}
function UpdateQuestion(question) {
   // var questionElement = $('#question' + question.PublicKey);
    var element = $('#elem-' + question.PublicKey);

    element.removeClass("ui-disabled");
    if (!question.Enabled || question.Featured)
        element.addClass("ui-disabled");

    element.removeClass("error_block");
    if (!question.Valid) {
        element.addClass("error_block");
    }

    if (question.Answered) {
        element.addClass("answered");
    } else {
        element.removeClass("answered");
    }

    SetErrorToQuestion(question, question.QuestionType == 0 ? null : question.GroupPublicKey, '');
}



function SetErrorToQuestion(question, key, error) {
    var questionElement = key ? $('#propagatedGroup' + key + ' #elem-' + question.PublicKey) : $('#elem-' + question.PublicKey);
    // questionElement.find('[data-valmsg-replace=true]').text(error);
    if (error + "" != "") {
        $('#error-' + question.PublicKey + ' p:first').text(error);
    }

}
function UpdateGroup(group) {
    if (group.FeaturedTitle) {
        $('#featured-title-' + group.PropogationKey).html(group.FeaturedTitle);
    }
    if (group.PropagatedGroups && group.PropagatedGroups.length > 0) {
        for (var p = 0; p < group.PropagatedGroups.length; p++) {
            if (group.PropagatedGroups[p].Questions) {
                for (var qp = 0; qp < group.PropagatedGroups[p].Questions.length; qp++) {
                    UpdateQuestion(group.PropagatedGroups[p].Questions[qp], group.PropagatedGroups[p].PropogationKey);
                }
            }
        }
    } else {
        if (group.Questions) {
            for (var i = 0; i < group.Questions.length; i++) {
                UpdateQuestion(group.Questions[i]);
            }
        }
    }
    if (group.Screens) {
        for (var j = 0; j < group.Screens.length; j++) {
            UpdateGroup(group.Screens[j]);
        }
    }
    if (group.PropagatedScreens) {
        for (var j = 0; j < group.PropagatedScreens.length; j++) {
            UpdateGroup(group.PropagatedScreens[j]);
        }
    }
}

function RemovePropagatedGroup(data, status, xhr) {
    var group = jQuery.parseJSON(data.responseText);

    var li = $('#propagatedGroup' + group.propagationKey);
    var parent = li.parent();
    $(li).remove();
    $(parent).listview('refresh');
    updateCounter();
}

function PropagatedGroup(data, status, xhr) {
    var group = jQuery.parseJSON(data.responseText);
    if (!group.error) {
        var templateDivPath = '#groupTemplate' + group.parentGroupPublicKey;
        var parent = $('#propagate-list-' + group.parentGroupPublicKey);
        var validator = parent.find('[data-valmsg-replace=true]');
        validator.text('');
        var template = $(templateDivPath).html();

        var str = template.replace(/00000000-0000-0000-0000-000000000000/gi, group.propagationKey);
        str = str.replace("${Number}", 50);
        var newGroup = $(str);

        var container = parent.find(" > li:last");

        if (container.length == 0) {
            parent.prepend(newGroup);
        } else {
            newGroup.insertAfter(container);
        }

     //   newGroup.trigger('pagecreate');
        $(parent).listview('refresh');

        updateCounter();

        $(parent).find('.propagated-list-item').each(function (i, el) {
            var index = (i + 1) + ')';
            $(this).find('h3 span').html(index);
            var screenId = $(this).attr('id').replace("propagatedGroup", "#screen-");
            $(screenId + ' .ui-footer h1 span').html(index);
        });
        UpdateCurrentGroup(group.group);
    } else {
        $('<div>').simpledialog2({
            mode: 'button',
            headerText: 'Propagation error',
            headerClose: true,
            buttonPrompt: group.error,
            buttons: {
                'OK': {
                    click: function () {

                    }
                }
            }
        });
    }
}
function updateCounter() {
    var all = $('#main').parent().find('.question').length;
    var disabled = $('#main').parent().find('.question.ui-disabled').length;
    var total = all - disabled;
    var answered = $('#main').parent().find('.question.answered').length;
    $('.ui-li-count.current').html(answered + "/" + total);
}
(function ($) {
    $.extend($.keyboard.layouts, { 'qwertyNoEnter': {
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

        'numOnly': {
            'default': [
				'{dec} {sign} {b}',
				'7 8 9',
				'4 5 6',
				'1 2 3',
				'{a} 0 {c}'
			]
        }

    });
    $.fn.disableAfterSubmit = function () {
        var anchors = this.find('a[disable-after-click=true]');
        anchors.click(function () {
            var button = $(this);
            setTimeout(function () {
                button.attr('href', '#');
            }, 0);
        });
    },
    $.fn.clear_form_elements = function () {

        $(this).find(':input').each(function () {
            var jThis = $(this);
            try {
                switch (this.type) {
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
                        jThis.controlgroup("refresh");
                        jThis.trigger("create");
                    case 'hidden':
                        break;
                }
            } catch (e) {
                alert($(this));
            }
        });

    },
     $.fn.hideInputsWithLongTap = function () {
         var virtualIcons = this.find('input[on-long-tap-open=true]');
         virtualIcons.each(function() {
             var target = $(this);
             target.css('display', 'none');
             var grabParentAreas = target.attr('grab-parent-areas');
             if (grabParentAreas) {
                 var level = parseInt(grabParentAreas);
                 if (level && level != NaN) {
                     var targetParent = target;
                     for (var i = 0; i < level; i++) {
                         targetParent = targetParent.parent();
                     }
                  /*   targetParent.bind('taphold', function() {
                         target.click();
                     });*/
                     targetParent.bind("contextmenu", function(e) {
                         target.click();
    return false;
});
//                     targetParent.mousedown(function(event) {
//                         switch (event.which) {        
//                         case 3:
//                             target.click();
//                             break;
//                         default:
//                             return;
//                         }
//                     });
                 }
             }
         });
     },
    $.fn.hideInputsWithVirtualKeyboard = function () {
        var virtualIcons = this.find('a[open-virtual-keyboar=true]');
        virtualIcons.each(function() {
            var button = $(this);
            var target = button.attr('target-input');
            var targetInput = $('#' + target);
            var label = $('[from=' + target + ']');
            targetInput.css('display', 'none');

            targetInput.change(function() {
                label.html(targetInput.val());
            });
            var grabParentAreas = button.attr('grab-parent-areas');
            var parentHandled = false;
            if (grabParentAreas) {
                var level = parseInt(grabParentAreas);
                if (level && level != NaN) {
                    var targetParent = button;
                    for (var i = 0; i < level; i++) {
                        targetParent = targetParent.parent();
                    }
                    targetParent.mouseup(function(event) {
                        switch (event.which) {
                        case 1:
                            targetInput.click();
                            break;
                        default:
                            return;
                        }
                    });
                    parentHandled = true;

                }
            }
            if (!parentHandled) {

                button.mouseup(function(event) {
                    switch (event.which) {
                    case 1:
                        targetInput.click();
                        break;
                    default:
                        return;
                    }
                });
            }
        });
    },
    $.fn.createKeyBoard = function (layout) {
        
       
        var keyboardInputs = this.find('input[draw-key-board=true][type=text], textarea[draw-key-board=true]');
        var numericInputs = this.find('input[draw-key-board=true][type!=text]');
       
    //    keyboardInputs.add(numericInputs).removeAttr("draw-key-board");

        createKeyBoard({ layout: 'qwertyNoEnter', min_width: '888px',css: {container: 'text'}  }, "dummyTxtKeyBoard",keyboardInputs);
       
        createKeyBoard({ layout: 'numOnly', min_width: null,css: {container: 'numeric'} }, "dummyNumericKeyBoard", numericInputs);
       
        function createKeyBoard(options, name, inputs) {

            var input = initInput(options, name);
            input.bind('accepted.keyboard', function(event) {
               // var jInput = $(this);
                var target = $('#' + input.attr('target-input'));
                target.val(input.val());
                input.attr('target-input', '');
                var keyboard = input.getkeyboard();
                keyboard.$preview.val('');
                target.change();
                if(target.attr('submit-form')!='false')
                    target.closest("form").submit();
            });
            input.bind('visible.keyboard', function(event) {
               // var jInput = $(this);
                var target = $('#' + input.attr('target-input'));
                var keyboard = input.getkeyboard();
                keyboard.$preview.val(target.val());
        //        alert(input.val());
                keyboard.$preview.caretToEnd();
            });
            inputs.click(function() {
             /*   if(input.attr('target-input') && input.attr('target-input')!='')
                    return;*/
                input.attr('target-input', this.id);
                input.focus();
             //   input.click();
               
            });
            return input;
        }

        function initInput(options, name) {
            var keyBoardInited = $('#' + name);
            if (keyBoardInited.length > 0)
                return keyBoardInited;
            var input = $('<input>').attr({
                type: 'text',
                style:'display:none',
                id: name,
                name: name
            });
            var kbOptions = {
                keyBinding: 'mousedown touchstart',
                position: {
                    of: null, // optional - null (attach to input/textarea) or a jQuery object (attach elsewhere)
                    my: 'center top',
                    at: 'center top',
                    at2: 'center bottom' // used when "usePreview" is false (centers the keyboard at the bottom of the input/textarea)
                },
                autoAccept: true,
                css: {
                    input: '',
                    container: '',
                    buttonDefault: '',
                    buttonHover: '',
                    buttonActive: '',
                    buttonDisabled: ''
                }
            };
            var extendedOptions = $.extend({ }, kbOptions, options);
            var mobileOptions = {
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
            };
            input.appendTo('body');
            input.keyboard(extendedOptions).addMobile(mobileOptions);
            return input;
        }
    };
    $.fn.initPage = function() {

        this.createKeyBoard();
     
        this.hideInputsWithVirtualKeyboard();
        this.hideInputsWithLongTap();
        this.disableAfterSubmit();
        var scripts = this.find('script');
/*for (var ix = 0; ix < scripts.length; ix++) {
    alert(scripts[ix].text);
   // eval(scripts[ix].text);
}*/

    };



})(jQuery);
jQuery(document).bind("pagecreate", function (e) {
    //disable scrolled content in order to avoid unnessesarry clicks in touch emulator mode

    // if browser supports touch events then fo nothing
    if ("ontouchend" in document) {
        return;
    }
    var elements = jQuery(e.target).find(":jqmData(iscroll)");
    if (!elements || elements.length <= 0)
        return;
    var scrolls = elements.data('iscrollview').iscroll;
    var disableClass = 'ui-disabled ui-disabled-opacity';
    scrolls.options.hideScrollbar = true;
  //  scrolls.options.handleClick = false;
    var originalOnTouchEndMethod = scrolls.options.onTouchEnd;
    scrolls.options.onTouchEnd = function (evt) {
        originalOnTouchEndMethod.call(this, evt);
        this.iscrollview.$scrollerContent.removeClass(disableClass);

    };

    var originalOnScrollMoveMethod = scrolls.options.onScrollMove;
    scrolls.options.onScrollMove = function (evt) {
        originalOnScrollMoveMethod.call(this, evt);

        var target = this.iscrollview.$scrollerContent;
        if (this.absDistY>40 && !target.hasClass(disableClass))
            target.addClass(disableClass);

    };
});
$(document).ready(function () {
    $('.next-question').live('click', function () {
        var id = $(this).attr('id').substr(4);
        var parent = $('#elem-' + id).parent();
        var nextqs = parent.nextAll('.question-frame');
        var next = null;
        for (var i = 0; i < nextqs.length; i++) {
            if ($(nextqs[i]).find('.ui-disabled.question-main-content').length == 0) {
                next = nextqs[i];
                break;
            }
        }
        if (next != null) {
            scrollToQuestion(next);
        }
    });
    $('#CompleteLink').live('click', function () {
        $.mobile.showPageLoadingMsg();
        var link = $(this).attr('link');
        var returnlink = $(this).attr('returnlink');
        GetCheck(link, returnlink);
    });
});
function scrollToQuestion(question) {
    var scrollContainer = $(question).parent().offsetParent();
    var position = scrollContainer.find('#scroller').offset().top - $(question).offset().top;
    var scroll = scrollContainer.data('iscrollview');
    if (!scroll)
        return;
    scroll.refresh();
    scroll.scrollTo(0, position, 1500, false);
}

$(window).bind("pagecontainercreate", function () {
    $.mobile.hashListeningEnabled = false;
});
$(document).bind('pagebeforeshow', function (event, data) {
    var doc = $(event.target);

    doc.initPage();
    
    doc.focus();
});

/*$(document).bind('pageremove', function (event, data) {
    //var doc = $(event.target);
    if (event.target.id === $.mobile.activePage.attr('id')) {
        $.mobile.activePage = $($('[data-id=main]').childrens()[0]);
    }
});*/
$(document).bind('pagehide', function(event, data) {

    $('.page-to-delete').remove();
});
$(document).bind('pagechange', function () {
    //   var groupId = location.href.substr(location.href.indexOf("group") + 6, 36);
    $("div.ui-block-a").click(function () {
        if (jQuery(this).find("div.ui-disabled div.ui-controlgroup-controls:visible").length > 0)
            jQuery(this).find("div.ui-disabled div.ui-controlgroup-controls:visible").hide();
        else
            if (jQuery(this).find("div.ui-disabled div.ui-controlgroup-controls:hidden").length > 0)
                jQuery(this).find("div.ui-disabled div.ui-controlgroup-controls:hidden").show();
    });
    
    /* if ($('#sidebar #ref-link-' + groupId).length > 0) {
    $('#sidebar .ui-li').each(function () {
    $(this).removeClass('ui-btn-active');
    });
    $('#ref-link-' + groupId).parents('li').addClass('ui-btn-active');
    }else {
    $('#sidebar .ui-li').each(function () {
    if ($(this).find('a').attr('href').indexOf("group") != -1)
    $(this).removeClass('ui-btn-active');
    });
    }*/
    if ($('.scrollHere').length > 0) {
        var q = $('.scrollHere');
        //  var target = $(q.replace('question', '#elem-'));
        scrollToQuestion(q);
        $(q).faderEffect();
        $('.scrollHere').removeClass('scrollHere');
    }

});

function isNumber(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

$.fn.faderEffect = function(options) {
    options = jQuery.extend({
            count: 3, // how many times to fadein
            speed: 500, // spped of fadein
            callback: false // call when done
        }, options);

    return this.each(function() {

        // if we're done, do the callback
        if (0 == options.count) {
            if ($.isFunction(options.callback)) options.callback.call(this);
            return;
        }

        // hide so we can fade in
        if ($(this).is(':visible')) $(this).hide();

        // fade in, and call again
        $(this).fadeIn(options.speed, function() {
            options.count = options.count - 1; // countdown
            $(this).faderEffect(options);
        });
    });
};

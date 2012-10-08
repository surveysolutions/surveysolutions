(function ($) {
    $.extend($.keyboard.layouts, {
        'qwertyNoEnter': {
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
    }),
     $.fn.getNumericKeyboard = function () {
         return this.find('#dummyNumericKeyBoard').getkeyboard();
     };
    $.fn.getTextKeyboard = function () {
        return this.find('#dummyTxtKeyBoard').getkeyboard();
    };
    $.fn.createKeyBoard = function (layout) {


        var keyboardInputs = this.find('input[draw-key-board=true][type=text], textarea[draw-key-board=true]');
        var numericInputs = this.find('input[draw-key-board=true][type!=text]');

        //    keyboardInputs.add(numericInputs).removeAttr("draw-key-board");

        createKeyBoard({ layout: 'qwertyNoEnter', min_width: '888px', css: { container: 'text'} }, "dummyTxtKeyBoard", keyboardInputs);

        createKeyBoard({ layout: 'numOnly', min_width: null, css: { container: 'numeric'} }, "dummyNumericKeyBoard", numericInputs);
        hideInputsWithVirtualKeyboard(this.find('a[open-virtual-keyboar=true]'));
        function hideInputsWithVirtualKeyboard(virtualIcons) {
            virtualIcons.each(function () {
                var button = $(this);
                var target = button.attr('target-input');
                var targetInput = $('#' + target);
                var label = $('[from=' + target + ']');
                targetInput.css('display', 'none');

                targetInput.change(function () {
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
                        targetParent.mouseup(function (event) {
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

                    button.mouseup(function (event) {
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
        };
        function createKeyBoard(options, name, inputs) {

            var input = initInput(options, name);
            input.bind('accepted.keyboard', function (event) {
                // var jInput = $(this);
                var target = $('#' + input.attr('target-input'));
                target.val(input.val());
                input.attr('target-input', '');
                var keyboard = input.getkeyboard();
                keyboard.$preview.val('');
                target.change();
                if (target.attr('submit-form') != 'false')
                    target.closest("form").submit();

               
            });
            input.bind('visible.keyboard', function (event) {
                // var jInput = $(this);
                var target = $('#' + input.attr('target-input'));
                var keyboard = input.getkeyboard();
                keyboard.$preview.val(target.val());
                //        alert(input.val());
                // keyboard.$preview.caretToEnd();
                keyboard.$preview[0].select();

                var popupKeyboard = keyboard.$preview.parent();
                popupKeyboard.find('[additional-comments="true"]').remove();
                var title = target.attr('question-text');
                
                if (title && title.length > 0)
                    popupKeyboard.prepend($('<p additional-comments="true">' + title + '</p>'));
            });
            inputs.click(function () {
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
                style: 'display:none',
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
            var extendedOptions = $.extend({}, kbOptions, options);
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
})(jQuery);
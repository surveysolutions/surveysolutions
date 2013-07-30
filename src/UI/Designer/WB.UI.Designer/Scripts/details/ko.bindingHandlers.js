define('ko.bindingHandlers',
['jquery', 'ko'],
function ($, ko) {
    var unwrap = ko.utils.unwrapObservable;



    /*! http://mths.be/placeholder v2.0.7 by @mathias */
    ; (function (window, document, $) {

        var isInputSupported = 'placeholder' in document.createElement('input'),
            isTextareaSupported = 'placeholder' in document.createElement('textarea'),
            prototype = $.fn,
            valHooks = $.valHooks,
            hooks,
            placeholder;

        if (isInputSupported && isTextareaSupported) {

            placeholder = prototype.placeholder = function () {
                return this;
            };

            placeholder.input = placeholder.textarea = true;

        } else {

            placeholder = prototype.placeholder = function () {
                var $this = this;
                $this
                    .filter((isInputSupported ? 'textarea' : ':input') + '[placeholder]')
                    .not('.placeholder')
                    .bind({
                        'focus.placeholder': clearPlaceholder,
                        'blur.placeholder': setPlaceholder
                    })
                    .data('placeholder-enabled', true)
                    .trigger('blur.placeholder');
                return $this;
            };

            placeholder.input = isInputSupported;
            placeholder.textarea = isTextareaSupported;

            hooks = {
                'get': function (element) {
                    var $element = $(element);
                    return $element.data('placeholder-enabled') && $element.hasClass('placeholder') ? '' : element.value;
                },
                'set': function (element, value) {
                    var $element = $(element);
                    if (!$element.data('placeholder-enabled')) {
                        return element.value = value;
                    }
                    if (value == '') {
                        element.value = value;
                        // Issue #56: Setting the placeholder causes problems if the element continues to have focus.
                        if (element != document.activeElement) {
                            // We can't use `triggerHandler` here because of dummy text/password inputs :(
                            setPlaceholder.call(element);
                        }
                    } else if ($element.hasClass('placeholder')) {
                        clearPlaceholder.call(element, true, value) || (element.value = value);
                    } else {
                        element.value = value;
                    }
                    // `set` can not return `undefined`; see http://jsapi.info/jquery/1.7.1/val#L2363
                    return $element;
                }
            };

            isInputSupported || (valHooks.input = hooks);
            isTextareaSupported || (valHooks.textarea = hooks);

            $(function () {
                // Look for forms
                $(document).delegate('form', 'submit.placeholder', function () {
                    // Clear the placeholder values so they don't get submitted
                    var $inputs = $('.placeholder', this).each(clearPlaceholder);
                    setTimeout(function () {
                        $inputs.each(setPlaceholder);
                    }, 10);
                });
            });

            // Clear placeholder values upon page reload
            $(window).bind('beforeunload.placeholder', function () {
                $('.placeholder').each(function () {
                    this.value = '';
                });
            });

        }

        function args(elem) {
            // Return an object of element attributes
            var newAttrs = {},
                rinlinejQuery = /^jQuery\d+$/;
            $.each(elem.attributes, function (i, attr) {
                if (attr.specified && !rinlinejQuery.test(attr.name)) {
                    newAttrs[attr.name] = attr.value;
                }
            });
            return newAttrs;
        }

        function clearPlaceholder(event, value) {
            var input = this,
                $input = $(input);
            if (input.value == $input.attr('placeholder') && $input.hasClass('placeholder')) {
                if ($input.data('placeholder-password')) {
                    $input = $input.hide().next().show().attr('id', $input.removeAttr('id').data('placeholder-id'));
                    // If `clearPlaceholder` was called from `$.valHooks.input.set`
                    if (event === true) {
                        return $input[0].value = value;
                    }
                    $input.focus();
                } else {
                    input.value = '';
                    $input.removeClass('placeholder');
                    input == document.activeElement && input.select();
                }
            }
        }

        function setPlaceholder() {
            var $replacement,
                input = this,
                $input = $(input),
                $origInput = $input,
                id = this.id;
            if (input.value == '') {
                if (input.type == 'password') {
                    if (!$input.data('placeholder-textinput')) {
                        try {
                            $replacement = $input.clone().attr({ 'type': 'text' });
                        } catch (e) {
                            $replacement = $('<input>').attr($.extend(args(this), { 'type': 'text' }));
                        }
                        $replacement
                            .removeAttr('name')
                            .data({
                                'placeholder-password': true,
                                'placeholder-id': id
                            })
                            .bind('focus.placeholder', clearPlaceholder);
                        $input
                            .data({
                                'placeholder-textinput': $replacement,
                                'placeholder-id': id
                            })
                            .before($replacement);
                    }
                    $input = $input.removeAttr('id').hide().prev().attr('id', id).show();
                    // Note: `$input[0] != input` now!
                }
                $input.addClass('placeholder');
                $input[0].value = $input.attr('placeholder');
            } else {
                $input.removeClass('placeholder');
            }
        }

    }(window, document, jQuery));

    ko.bindingHandlers.popover = {
        update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var options = ko.utils.unwrapObservable(valueAccessor());
            if (options) {
                //$(element).attr('data-content', options.content || '');
                $(element).attr('data-title', options.title || 'Title');
                $(element).attr('data-placement', options.placement || 'right');
                $(element).attr('data-trigger', options.trigger || 'click');
                $(element).popover({ html: false });
            } else {
                $(element).removeAttr('data-title');
                $(element).removeAttr('data-placement');
                $(element).removeAttr('data-trigger');
                $(element).popover('destroy');
            }
        }
    };

    ko.bindingHandlers.expand = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            if ($(element).hasClass('ui-expander')) {
                var expander = element;
                var head = $(expander).find('.ui-expander-head');
                var content = $(expander).find('.ui-expander-content');

                $(head).click(function () {
                    $(head).toggleClass('ui-expander-head-collapsed');
                    $(content).toggle();
                });
            }
        }
    };

    ko.bindingHandlers.autoGrowArea = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            ko.applyBindingsToNode(element, { value: valueAccessor(), valueUpdate: 'afterkeydown' });
            $(element).autogrow();
        }
    };

    ko.bindingHandlers.checkedButtons = {
        init: function(element, valueAccessor, allBindingsAccessor) {
            var type = element.getAttribute('data-toggle') || 'radio';
            var updateHandler = function() {
                var valueToWrite;
                var isActive = !!~element.className.indexOf('active');
                var dataValue = element.getAttribute('data-value');
                if (type == "checkbox") {
                    valueToWrite = !isActive;
                } else if (type == "radio" && !isActive) {
                    valueToWrite = dataValue;
                } else {
                    return; // "checkedButtons" binding only responds to checkbox and radio data-toggle attribute
                }

                var modelValue = valueAccessor();
                if ((type == "checkbox") && (ko.utils.unwrapObservable(modelValue) instanceof Array)) {
                    // For checkboxes bound to an array, we add/remove the checkbox value to that array
                    // This works for both observable and non-observable arrays
                    var existingEntryIndex = ko.utils.arrayIndexOf(ko.utils.unwrapObservable(modelValue), dataValue);
                    if (!isActive && (existingEntryIndex < 0))
                        modelValue.push(dataValue);
                    else if (isActive && (existingEntryIndex >= 0))
                        modelValue.splice(existingEntryIndex, 1);
                } else {
                    if (modelValue() !== valueToWrite) {
                        modelValue(valueToWrite);
                    }
                }
            };

            ko.utils.registerEventHandler(element, "click", updateHandler);
        },
        update: function(element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            var type = element.getAttribute('data-toggle') || 'radio';

            if (type == "checkbox") {
                if (value instanceof Array) {
                    // When bound to an array, the checkbox being checked represents its value being present in that array
                    if (ko.utils.arrayIndexOf(value, element.getAttribute('data-value')) >= 0) {
                        ko.utils.toggleDomNodeCssClass(element, 'active', true);
                        ko.utils.toggleDomNodeCssClass(element, 'btn-info', true);
                    } else {
                        ko.utils.toggleDomNodeCssClass(element, 'active', false);
                        ko.utils.toggleDomNodeCssClass(element, 'btn-info', false);
                    }

                } else {
                    // When bound to anything other value (not an array), the checkbox being checked represents the value being trueish
                    ko.utils.toggleDomNodeCssClass(element, 'active', value);
                    ko.utils.toggleDomNodeCssClass(element, 'btn-info', value);
                }
            } else if (type == "radio") {
                ko.utils.toggleDomNodeCssClass(element, 'active', element.getAttribute('data-value') == value);
                ko.utils.toggleDomNodeCssClass(element, 'btn-info', element.getAttribute('data-value') == value);
            }
        }
    };
    
    // escape
    //---------------------------
    ko.bindingHandlers.escape = {
        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var command = valueAccessor();
            $(element).keyup(function (event) {
                if (event.keyCode === 27) { // <ESC>
                    command.call(viewModel, viewModel, event);
                }
            });
        }
    };

    // hidden
    //---------------------------
    ko.bindingHandlers.hidden = {
        update: function (element, valueAccessor) {
            var value = unwrap(valueAccessor());
            ko.bindingHandlers.visible.update(element, function () { return !value; });
        }
    };

    // checboxImage
    //---------------------------
    ko.bindingHandlers.checkboxImage = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var $el = $(element),
                settings = valueAccessor();

            $el.addClass('checkbox');

            $el.click(function () {
                if (settings.checked) {
                    settings.checked(!settings.checked());
                }
            });

            ko.bindingHandlers.checkboxImage.update(
                element, valueAccessor, allBindingsAccessor, viewModel);
        },
        update: function (element, valueAccessor) { 
            var $el = $(element),
                settings = valueAccessor(),
                enable = (settings.enable !== undefined) ? unwrap(settings.enable()) : true,
                checked = (settings.checked !== undefined) ? unwrap(settings.checked()) : true;

            $el.prop('disabled', !enable);

            checked ? $el.addClass('selected') : $el.removeClass('selected');
        }
    };

    // starRating
    //---------------------------
    ko.bindingHandlers.starRating = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            // Create the span's (only do in init)
            for (var i = 0; i < 5; i++) {
                $('<span>').appendTo(element);
            }

            ko.bindingHandlers.starRating.update(element, valueAccessor, allBindingsAccessor, viewModel);
        },

        update: function (element, valueAccessor, allBindingsAccessor) {
            // Give the first x stars the 'chosen' class, where x <= rating
            var ratingObservable = valueAccessor(),
                allBindings = allBindingsAccessor(),
                enable = (allBindings.enable !== undefined) ? unwrap(allBindings.enable) : true;

            // Toggle the appropriate CSS classes
            if (enable) {
                $(element).addClass('starRating').removeClass('starRating-readonly');
            }else {
                $(element).removeClass('starRating').addClass('starRating-readonly');
            }
            
            // Wire up the event handlers, if enabled
            if (enable) {
                // Handle mouse events on the stars
                $('span', element).each(function (index) {
                    var $star = $(this);

                    $star.hover(
                        function () {
                            $star.prevAll().add(this).addClass('hoverChosen');
                        },
                        function () {
                            $star.prevAll().add(this).removeClass('hoverChosen');
                        });

                    $star.click(function () {
                        //var ratingObservable = valueAccessor(); // Get the associated observable
                        ratingObservable(index + 1); // Write the new rating to it
                    });
                });
            }
            
            // Toggle the chosen CSS class (fills in the stars for the rating)
            $('span', element).each(function (index) {
                $(this).toggleClass('chosen', index < ratingObservable());
            });
        }
    };
});
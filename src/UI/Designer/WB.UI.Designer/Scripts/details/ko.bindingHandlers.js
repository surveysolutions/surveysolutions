define('ko.bindingHandlers',
['jquery', 'ko'],
function ($, ko) {
    var unwrap = ko.utils.unwrapObservable;

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
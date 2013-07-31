// Based on Knockout Bindings for TinyMCE
// https://github.com/SteveSanderson/knockout/wiki/Bindings---tinyMCE
// Initial version by Ryan Niemeyer. Updated by Scott Messinger, Frederik Raabye, Thomas Hallock and Drew Freyling.

(function ($) {
    var instances_by_id = {} // needed for referencing instances during updates.
    , init_id = 0;           // generated id increment storage

    ko.bindingHandlers.ace = {
        init: function (element, valueAccessor, allBindingsAccessor, context) {

            var init_arguments = arguments;
            var options = allBindingsAccessor().aceOptions || {};
            var modelValue = valueAccessor();
            var value = ko.utils.unwrapObservable(valueAccessor());
            var el = $(element);

            // Ace attaches to the element by DOM id, so we need to make one for the element if it doesn't have one already.
            if (!element.id) {
                element.id = 'knockout-ace-' + init_id;
                init_id = init_id + 1;
            }

            var editor = ace.edit(element.id);

            if (options.theme) editor.setTheme("ace/theme/" + options.theme);
            if (options.mode) editor.getSession().setMode("ace/mode/" + options.mode);

            editor.setValue(value);
            editor.gotoLine(0);
            editor.renderer.setShowGutter(false);
            editor.setShowPrintMargin(false);
            
            var heightUpdateFunction = function () {

                // http://stackoverflow.com/questions/11584061/
                var newHeight =
                          editor.getSession().getScreenLength()
                          * editor.renderer.lineHeight
                          + (editor.renderer.$horizScroll ? editor.renderer.scrollBar.getWidth() : 0);
                var editorElement = $(editor.container);
                var editorContainer = editorElement.parent();
                $(editorElement).height(newHeight.toString() + "px");
                $(editorContainer).height(newHeight.toString() + "px");

                // This call is required for the editor to fix all of
                // its inner structure for adapting to a change in size
                editor.resize();
            };

            editor.getSession().on("change", function (delta) {
                heightUpdateFunction();
                if (ko.isWriteableObservable(modelValue)) {
                    modelValue(editor.getValue());
                }
            });

            instances_by_id[element.id] = editor;
        },
        update: function (element, valueAccessor, allBindingsAccessor, context) {
            var el = $(element);
            var value = ko.utils.unwrapObservable(valueAccessor());
            var id = el.attr('id');

            //handle programmatic updates to the observable
            // also makes sure it doesn't update it if it's the same.
            // otherwise, it will reload the instance, causing the cursor to jump.
            if (id !== undefined && id !== '' && instances_by_id.hasOwnProperty(id)) {
                var editor = instances_by_id[id];
                var content = editor.getValue();
                if (content !== value) {
                    editor.setValue(value);
                    editor.gotoLine(0);
                }
            }
        }
    };
}(jQuery));
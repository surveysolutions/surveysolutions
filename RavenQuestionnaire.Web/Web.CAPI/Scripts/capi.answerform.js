(function ($) {
    $.answerForm = function (el, options) {
        var base = this, o;
        base.$el = $(el);
        base.el = el;
        // Add a reverse reference to the DOM object
        base.$el.data("answerForm", base);
        base.init = function () {
            base.options = o = $.extend(true, {}, $.keyboard.defaultOptions, options);
            var questionnaireId = base.$el.attr('answer-form-id');
            var formName = base.$el.attr('answer-form-name');
            if (!formName || formName == '')
                formName = "answer-form";
            var formId = formName + "-" + questionnaireId;
            if ($('#' + formId).length == 0) {
                var $form = $("<form id='" + formId + "'></form>");
                $form.append("<input type='hidden' value='" + questionnaireId + "' name='QuestionnaireId' />");
                $form.append("<input type='hidden' value='' name='PropogationPublicKey' />");
                $form.append("<input type='hidden' value='' name='PublicKey' />");
                $('body').append($form);
            }
            base.$el.css('display', 'none');
            base.$el.click(function () {

            });
        };
        // Run initializer
        base.init();
    };
    $.fn.getAnswerForm = function () {
        return this.data("answerForm");
    };
    $.fn.answerForm = function (options) {
        return this.each(function () {
            if (!$(this).data('answerForm')) {
                (new $.answerForm(this, options));
            }
        });
    };
})(jQuery);

jQuery(document).bind("pagecreate", function (e) {
    "use strict";

    // In here, e.target refers to the page that was created (it's the target of the pagecreate event)
    // So, we can simply find elements on this page that match a selector of our choosing, and call
    // our plugin on them.

    // The find() below returns an array of elements within a newly-created page that have
    // the data-iscroll attribute. The Widget Factory will enumerate these and call the widget 
    // _create() function for each member of the array.
    // If the array is of zero length, then no _create() fucntion is called.
    var elements = jQuery(e.target).find(":jqmData(answer-form)");
    elements.answerForm();
});
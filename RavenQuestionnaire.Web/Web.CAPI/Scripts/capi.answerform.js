(function ($) {
    $.answerForm = function (el, options) {
        var base = this, o;
        base.$el = $(el);
        base.el = el;
        base.targetForm = null;
        // Add a reverse reference to the DOM object
        base.$el.data("answerForm", base);
        base.init = function () {
            base.options = o = $.extend(true, {}, $.keyboard.defaultOptions, options);
            var questionId = base.$el.attr('question-item');
            base.targetForm = $('[answer-form=' + questionId + ']');
            if (!base.targetForm || base.targetForm.length == 0)
                return;
            base.$el.click(function () {
                var hidden = base.targetForm.find('input[name=PropogationPublicKey]');
                var formType = base.targetForm.find('input[name=QuestionType]').val();
                var propagationKey = base.$el.attr('question-propagation-key');
                hidden.attr('value', propagationKey);
                base[formType]();


            });
        };
        base.SingleOption = function () {
            var questionAnswer = base.$el.attr('question-answer-key');
            $("input[type='radio']").attr('checked', false).checkboxradio("refresh");
            var inputForSelect = base.targetForm.find('input[value=' + questionAnswer + ']');
            inputForSelect.attr('checked', true).checkboxradio("refresh");
        };
        base.YesNo = function () {
            base.SingleOption();
        };
        base.DropDownList = function () {
            var questionAnswer = base.$el.attr('question-answer-key');
            var inputForSelect = base.targetForm.find('option[value=' + questionAnswer + ']');
            inputForSelect.attr('selected', true);
            inputForSelect.parent().selectmenu("refresh");
        };
        base.MultyOption = function () {
            var questionAnswers = base.$el.attr('question-answer-key').split(';');
            $("input[type='checkbox']").removeAttr('checked').checkboxradio("refresh");
            $.each(questionAnswers, function (index, value) {
                var inputForSelect = base.targetForm.find("input[name='Answers[" + value + "].Selected']");
                inputForSelect.attr('checked', true).checkboxradio("refresh");
            });
        };
        base.Numeric = function () {
        };
        base.DateTime = function () {
        };
        base.GpsCoordinates = function () {
        };
        base.Text = function () {
        };
        base.Percentage = function () {
        };
        base.AutoPropagate = function () {
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
    var elements = jQuery(e.target).find("[question-item]");
    elements.answerForm();
});
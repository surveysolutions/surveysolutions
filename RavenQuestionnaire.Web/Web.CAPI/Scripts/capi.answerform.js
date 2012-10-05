(function ($) {
    $.gridCell = function (el, options) {
        var base = this, o;
        base.$el = $(el);
        base.el = el;
        //     base.targetForm = null;
        base.propagationKey = null;
        base.questionId = null;
        base.answerForm = null;
        base.commentForm = null;
        // Add a reverse reference to the DOM object
        base.$el.data("gridCell", base);
        base.init = function () {
            base.options = o = $.extend(true, {}, {}, options);
            base.questionId = base.$el.attr('question-item');
            base.propagationKey = base.$el.attr('question-propagation-key');
            /* var eventName = base.$el.attr('event-name');
            if (eventName && base.$el.parent()[eventName])
            base.activeEvent = eventName;*/
            /* var formName = base.$el.attr('form-name');
            if (formName && formName != '') {
            base.formName = formName;
            }*/
            var jTargetCommentForm = $('[comment-form=' + base.questionId + ']');
            if (jTargetCommentForm && jTargetCommentForm.length > 0) {
                jTargetCommentForm.answerForm();
                base.commentForm = jTargetCommentForm.getAnswerForm();
                if (base.commentForm) {

                    // base.commentForm = base.commentForm.getAnswerForm();

                    base.$el.bind("contextmenu", function (e) {
                        base.commentForm.open(e, base);
                    });
                }
            }

            var jTargetAnswerForm = $('[answer-form=' + base.questionId + ']');
            if (jTargetAnswerForm && jTargetAnswerForm.length > 0) {
                jTargetAnswerForm.answerForm();
                base.answerForm = jTargetAnswerForm.getAnswerForm();
                if (base.answerForm) {
                    // base.answerForm = base.answerForm.getAnswerForm();

                    base.$el.bind("click", function (e) {
                        base.answerForm.open(e, base);
                    });
                }
            }

        };
        base.init();
    };
    $.answerForm = function (el, options) {
        var base = this, o;
        base.$el = $(el);
        base.el = el;
        base.questionId = null;
        // Add a reverse reference to the DOM object
        base.$el.data("answerForm", base);
        base.init = function () {
            base.questionId = base.$el.find('input[name=PublicKey]').val();
            base.options = o = $.extend(true, {}, {}, options);
            //    if (!base.targetForm.data("answerFormInit")) {
            base.$el.ajaxSuccess(function (e, xhr, settings) {
                if ($.inArray('json', settings.dataTypes) < 0)
                    return;
                if (e.target !== base.el)
                    return;
                var jsonResponse = jQuery.parseJSON(xhr.responseText);
                if (!jsonResponse.Grid)
                    return;
                base.AjaxSuccess(jQuery.parseJSON(xhr.responseText).Grid, getParameterByName('PropogationPublicKey', settings.data));
            });
            /*    base.targetForm.data("answerFormInit", true);
            }*/


        };
        base.open = function (e, target) {
            var hidden = base.$el.find('input[name=PropogationPublicKey]');
            var formType = base.$el.find('input[name=QuestionType]').val();
            hidden.val(target.propagationKey);
            target.$el.addClass('ui-bar-e');
            base[formType](e, target);
        };
        function getParameterByName(name, settings) {
            name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
            var regexS = "[\\?&]" + name + "=([^&#]*)";
            var regex = new RegExp(regexS);
            var results = regex.exec(settings);
            if (results == null)
                return "";
            else
                return decodeURIComponent(results[1].replace(/\+/g, " "));
        }
        base.AjaxSuccess = function (data, rowKey) {
            $.each(data.Row, function (index, value) {
                if (value.PropagationKey != rowKey)
                    return;
                $.each(value.Answers, function (answerIndex, answerValue) {
                    var target = $('[question-propagation-key=' + rowKey + '][question-item=' + answerValue.PublicKey + ']');
                    //       var targetPanel = target.parent();
                    target.removeClass('ui-bar-e');
                    var containers = target.find('span');
                    $(containers[0]).text(answerValue.AnswerString);
                    $(containers[1]).text(answerValue.Comments);
                    target.attr('question-comment-value', answerValue.Comments);
                    target.attr('question-answer-key', answerValue.AnswerPublicKey);
                    target.attr('question-answer-value', answerValue.AnswerString);
                    if (answerValue.Enabled) {
                        target.removeClass('ui-disabled');
                    } else {
                        target.addClass('ui-disabled');
                    }
                    if (answerValue.Answered) {
                        target.addClass('answered');
                    } else {
                        target.removeClass('answered');
                    }
                    if (answerValue.Valid) {
                        target.removeClass('error_block');
                    } else {
                        target.addClass('error_block');
                    }
                });
            });
        };
        base.openDialog = function (x, y) {
            $('#grid-popup-' + base.questionId).popup('open', x, y);
        };
        base.SingleOption = function (e, target) {
            var questionAnswer = target.$el.attr('question-answer-key');
            base.$el.find("input[type='radio']").attr('checked', false).checkboxradio("refresh");
            var inputForSelect = base.$el.find('input[value=' + questionAnswer + ']');
            inputForSelect.attr('checked', true).checkboxradio("refresh"); /*target.$el.offset().top + 'px'*/
            base.openDialog(target.$el.offset().left, target.$el.offset().top);
        };

        base.YesNo = function (e, target) {
            base.SingleOption(e, target);
        };
        base.DropDownList = function (e, target) {
            var questionAnswer = target.$el.attr('question-answer-key');
            var inputForSelect = base.$el.find('option[value=' + questionAnswer + ']');
            inputForSelect.attr('selected', true);
            inputForSelect.parent().selectmenu("refresh");
            base.openDialog();
        };
        base.MultyOption = function (e, target) {
            var questionAnswers = target.$el.attr('question-answer-key').split(';');
            base.$el.find("input[type='checkbox']").removeAttr('checked').checkboxradio("refresh");
            $.each(questionAnswers, function (index, value) {
                var inputForSelect = base.$el.find("input[name='Answers[" + value + "].Selected']");
                inputForSelect.attr('checked', true).checkboxradio("refresh");
            });
            base.openDialog();
        };
        base.Numeric = function (e, target) {
            var questionAnswer = $.trim(target.$el.attr('question-answer-value'));

            var targetInput = base.$el.find("input[type='num']");

            targetInput.val(questionAnswer);
            targetInput.click();
        };
        base.DateTime = function (e, target) {
            var questionAnswer = $.trim(target.$el.attr('question-answer-value'));
            var targetInput = base.targetForm.find("input[type='text']");

            targetInput.val(questionAnswer);
            targetInput.click();
        };
        base.GpsCoordinates = function (e, target) {
        };
        base.Text = function (e, target) {
            var questionAnswer = $.trim(target.$el.attr('question-answer-value'));
            var targetInput = base.$el.find("input[type='text']");
            targetInput.val(questionAnswer);
            targetInput.click();
        };
        base.Comments = function (e, target) {
            var questionAnswer = $.trim(target.$el.attr('question-comment-value'));
            var targetInput = base.$el.find("input[type='text']");
            targetInput.val(questionAnswer);
            targetInput.click();
        };
        base.Percentage = function (e, target) {
        };
        base.AutoPropagate = function (e, target) {
            base.Numeric(e, target);
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
    $.fn.getGridCell = function () {
        return this.data("gridCell");
    };
    $.fn.gridCell = function (options) {
        return this.each(function () {
            if (!$(this).data('gridCell')) {
                (new $.gridCell(this, options));
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
    elements.gridCell();
});
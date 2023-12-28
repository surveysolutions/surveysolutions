<template>
    <div class="row table-holder">
        <div class="col-xs-offset-5 col-xs-6">
            <div class="form-group singleline-group" :class="{ 'has-error': !questionForm.maxAnswerCount.$valid }">
                <label for="edit-question-max-answers-number">{{
                    $t('QuestionnaireEditor.QuestionMaxNumberOfAnswers') }}</label>
                <input id="edit-question-max-answers-number" maxlength="9" name="maxAnswerCount" type="text"
                    ng-pattern="/^\d+$/" ng-model="activeQuestion.maxAnswerCount"
                    class="form-control small-numeric-input" />
                <p class="help-block ng-cloak" v-show="questionForm.maxAnswerCount.$error.pattern">{{
                    $t('QuestionnaireEditor.QuestionOnlyInts') }}
                </p>
            </div>
        </div>
    </div>
</template>

<script>

export default {
    name: 'TextListQuestion',
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {

        }
    },
}
</script>

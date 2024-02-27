<template>
    <div class="row">
        <div class="col-md-5"></div>
        <div class="col-md-5 inline-inputs">
            <div class="form-group singleline-group checkbox-in-column" :class="{ 'has-error': !isValidMaxAnswerCount }">
                <label for="edit-question-max-answers-number">
                    {{ $t('QuestionnaireEditor.QuestionMaxNumberOfAnswers') }}
                </label>
                <input id="edit-question-max-answers-number" maxlength="9" name="maxAnswerCount" type="number"
                    inputmode="numeric" v-number v-model.number="activeQuestion.maxAnswerCount"
                    class="form-control small-numeric-input" />
                <p class="help-block ng-cloak" v-show="!isValidMaxAnswerCount">
                    {{ $t('QuestionnaireEditor.QuestionOnlyInts') }}
                </p>
            </div>
        </div>
    </div>
</template>

<script>

import { isInteger } from '../../../../helpers/number';

export default {
    name: 'TextListQuestion',
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {
            valid: true
        }
    },
    computed: {
        isValidMaxAnswerCount() {
            return this.activeQuestion.maxAnswerCount == null || this.activeQuestion.maxAnswerCount == '' || isInteger(this.activeQuestion.maxAnswerCount);
        }
    }
}
</script>

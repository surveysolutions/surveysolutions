<template>
    <div class="row">
        <div class="col-md-6">
            <div class="checkbox-in-column">
                <input id="cb-is-timestamp" type="checkbox" class="wb-checkbox" v-model="activeQuestion.isTimestamp" />
                <label for="cb-is-timestamp"><span></span>{{ $t('QuestionnaireEditor.QuestionCurrentTime') }}</label>
                <help link="isTimestamp"></help>
            </div>
        </div>
        <div class="col-md-5 ng-scope inline-inputs" v-if="!activeQuestion.isTimestamp">
            <div class="form-group checkbox checkbox-in-column">
                <label for="dt-default-date">{{ $t('QuestionnaireEditor.QuestionDefaultDate') }}</label>
                <input id="dt-default-date" type="text" jqdatepicker v-model="activeQuestion.defaultDate"
                    class="form-control small-date-input" />
                <help link="defaultDate"></help>
            </div>
        </div>
    </div>
</template>

<script>

import Help from './../Help.vue'

export default {
    name: 'DateTimeQuestion',
    components: {
        Help,
    },
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {

        }
    },
}
</script>

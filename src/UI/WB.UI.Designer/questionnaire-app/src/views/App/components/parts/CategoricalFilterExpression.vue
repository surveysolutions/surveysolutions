<template>
    <div class="add-filter-label" v-if="showFilter != true && activeQuestion.isCascade != true">
        <button type="button" class="btn btn-lg btn-link" @click="showFilter = true">{{
            $t('QuestionnaireEditor.AddFilter') }}</button>
    </div>

    <div class="row" v-show="showFilter == true && activeQuestion.isCascade != true">
        <div class="form-group col-xs-11">
            <label class="wb-label" for="optionsFilterExpression">{{ $t('QuestionnaireEditor.QuestionFilter') }}</label>

            <ExpressionEditor v-if="activeQuestion.linkedToEntityId != null && linkedToEntity.type != 'textlist'"
                v-model="activeQuestion.linkedFilterExpression" mode="expression"></ExpressionEditor>
            <ExpressionEditor v-if="!(activeQuestion.linkedToEntityId != null && linkedToEntity.type != 'textlist')"
                v-model="activeQuestion.optionsFilterExpression" mode="expression"></ExpressionEditor>

        </div>

        <div class="form-group col-xs-1">
            <button type="button" class="btn cross filter-cross"
                @click="showFilter = false; activeQuestion.linkedFilterExpression = ''; activeQuestion.optionsFilterExpression = '';"></button>
        </div>
    </div>
</template>

<script>

import ExpressionEditor from '../ExpressionEditor.vue';

export default {
    name: 'CategoricalFilterExpression',
    components: {
        ExpressionEditor,
    },
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {
            showFilter: null,
        };
    },
    watch: {
        'activeQuestion.linkedFilterExpression'(newValue, oldValue) {
            if (!oldValue && !this.showFilter)
                this.calculateShowFilter();
        },
        'activeQuestion.optionsFilterExpression'(newValue, oldValue) {
            if (!oldValue && !this.showFilter)
                this.calculateShowFilter();
        },
    },
    async beforeMount() {
        this.calculateShowFilter();
    },
    computed: {
        linkedToEntity() {
            if (this.activeQuestion.linkedToEntityId == null)
                return {};

            const sourceQuestion = this.activeQuestion.sourceOfLinkedEntities.find(
                p => p.id == this.activeQuestion.linkedToEntityId && p.isSectionPlaceHolder != true
            );
            return sourceQuestion != undefined ? sourceQuestion : {};
        },
    },
    methods: {
        calculateShowFilter() {
            this.showFilter = (this.activeQuestion.linkedFilterExpression && this.activeQuestion.linkedFilterExpression !== '')
                || (this.activeQuestion.optionsFilterExpression && this.activeQuestion.optionsFilterExpression !== '') ? true : false;
        }
    }
};
</script>

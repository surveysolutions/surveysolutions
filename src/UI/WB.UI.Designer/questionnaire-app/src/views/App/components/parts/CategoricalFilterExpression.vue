<template>
    <div class="add-filter-label" v-if="activeQuestion.showFilterInput != true && activeQuestion.isCascade != true">
        <button type="button" class="btn btn-lg btn-link" @click="activeQuestion.showFilterInput = true"
            v-t="{ path: 'QuestionnaireEditor.AddFilter' }"></button>
    </div>

    <div class="row" v-show="activeQuestion.showFilterInput == true && activeQuestion.isCascade != true">
        <div class="form-group col-xs-11">
            <label class="wb-label" for="optionsFilterExpression"
                v-t="{ path: 'QuestionnaireEditor.QuestionFilter' }"></label>
            
            <ExpressionEditor v-if="activeQuestion.isLinked && linkedToEntity.type != 'textlist'"
                    v-model="activeQuestion.linkedFilterExpression" mode="expression"></ExpressionEditor>
            <ExpressionEditor v-if="!(activeQuestion.isLinked && linkedToEntity.type != 'textlist')"
                    v-model="activeQuestion.optionsFilterExpression" mode="expression"></ExpressionEditor>
            
        </div>

        <div class="form-group col-xs-1">
            <button type="button" class="btn cross filter-cross"
                @click="activeQuestion.showFilterInput = false; activeQuestion.linkedFilterExpression = ''; activeQuestion.optionsFilterExpression = ''; questionForm.$setDirty();"></button>
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
        return {};
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
    }
};
</script>

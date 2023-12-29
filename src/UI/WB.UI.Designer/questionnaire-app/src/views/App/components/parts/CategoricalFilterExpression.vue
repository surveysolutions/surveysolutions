<template>
    <div class="add-filter-label" v-if="activeQuestion.showFilterInput != true && activeQuestion.isCascade != true">
        <button type="button" class="btn btn-lg btn-link" @click="activeQuestion.showFilterInput = true"
            v-t="{ path: 'QuestionnaireEditor.AddFilter' }"></button>
    </div>

    <div class="row" v-show="activeQuestion.showFilterInput == true && activeQuestion.isCascade != true">
        <div class="form-group col-xs-11">
            <label class="wb-label" for="optionsFilterExpression"
                v-t="{ path: 'QuestionnaireEditor.QuestionFilter' }"></label>
            <div class="pseudo-form-control">
                <div id="optionsFilterExpression"
                    v-if="activeQuestion.isLinked && activeQuestion.linkedToEntity.type != 'textlist'"
                    ui-ace="{ onLoad : aceLoaded, require: ['ace/ext/language_tools'] }"
                    v-model="activeQuestion.linkedFilterExpression"></div>
                <div id="optionsFilterExpression"
                    v-if="!(activeQuestion.isLinked && activeQuestion.linkedToEntity.type != 'textlist')"
                    ui-ace="{ onLoad : aceLoadedForOptionFilter, require: ['ace/ext/language_tools'] }"
                    v-model="activeQuestion.optionsFilterExpression"></div>
            </div>
        </div>

        <div class="form-group col-xs-1">
            <button type="button" class="btn cross filter-cross"
                @click="activeQuestion.showFilterInput = false; activeQuestion.linkedFilterExpression = ''; activeQuestion.optionsFilterExpression = ''; questionForm.$setDirty();"></button>
        </div>
    </div>
</template>

<script>
export default {
    name: 'CategoricalFilterExpression',
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {};
    }
};
</script>

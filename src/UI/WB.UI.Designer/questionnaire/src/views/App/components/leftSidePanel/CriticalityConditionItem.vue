<template>
    <div class="form-group validation-group" style="background:000;">
        <div>
            <div class="criticality-group-marker"></div>
            <label>
                {{ $t('QuestionnaireEditor.SideBarCriticalityConditionExpression') }} {{ index + 1 }}
                <help link="criticalityConditionExpression" />
            </label>

            <button type="button" class="btn delete-btn-sm delete-validation-condition" @click="remove()"
                v-if="!isReadOnlyForUser" tabindex="-1"></button>

            <ExpressionEditor v-model="criticalityCondition.edit.expression" mode="expression" focusable="true"
                aria-label="Expression" />

            <label class="validation-message">
                {{ $t('QuestionnaireEditor.SideBarCriticalityConditionMessage') }}
                <help link="criticalityConditionMessage" />
            </label>
            <ExpressionEditor v-model="criticalityCondition.edit.message" aria-label="Message" />
        </div>
        <div v-if="isDescriptionVisible()">
            <label class="validation-message">
                {{ $t('QuestionnaireEditor.SideBarCriticalityConditionDescription') }}
            </label>
            <textarea type="text" v-model="criticalityCondition.edit.description"
                class="form-control macros-description" v-autosize aria-label="Description">
                </textarea>
            <hr style="margin-top:0px; margin-bottom:0px;">
        </div>
        <div class="actions" v-if="isDirty" style="padding-top: 10px;">
            <button v-if="!isReadOnlyForUser" class="btn lighter-hover" @click="save()">{{
                $t('QuestionnaireEditor.Save') }}
            </button>
            <button type="button" class="btn lighter-hover" @click="cancel()">{{
                $t('QuestionnaireEditor.Cancel') }}</button>
            <button class="btn btn-default pull-right" v-if="isDescriptionEmpty()" type="button"
                @click="toggleDescription()">
                {{ isDescriptionVisible() ? $t('QuestionnaireEditor.SideBarHideDescription') :
                    $t('QuestionnaireEditor.SideBarShowDescription') }}
            </button>
        </div>
    </div>

</template>

<script>
import ExpressionEditor from '../ExpressionEditor.vue';
import Help from '../Help.vue'
import { isEmpty, cloneDeep } from 'lodash'
import { updateCriticalityCondition, deleteCriticalityCondition } from '../../../../services/criticalityConditionsService';
import { createQuestionForDeleteConfirmationPopup } from '../../../../services/utilityService'

export default {
    name: 'CriticalityConditionItem',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    components: {
        ExpressionEditor,
        Help,
    },
    props: {
        questionnaireId: { type: String, required: true },
        criticalityCondition: { type: Object, required: true },
        index: { type: Number, required: true }
    },
    data() {
        return {}
    },
    computed: {
        isDirty() {
            return this.criticalityCondition.message !== this.criticalityCondition.edit.message
                || this.criticalityCondition.expression !== this.criticalityCondition.edit.expression
                || this.criticalityCondition.description !== this.criticalityCondition.edit.description
                || (this.criticalityCondition.edit.description == '' && this.criticalityCondition.edit.isDescriptionVisible);

        },
        isInvalid() {
            return (this.categories.message) ? false : true;
        },
    },
    methods: {
        remove() {
            const params = createQuestionForDeleteConfirmationPopup(
                this.criticalityCondition.edit.message || this.$t('QuestionnaireEditor.SideBarCriticalityConditionNoMessage')
            );

            params.callback = confirm => {
                if (confirm) {
                    deleteCriticalityCondition(this.questionnaire.questionnaireId, this.criticalityCondition.id);
                }
            };

            this.$confirm(params);
        },
        toggleDescription() {
            this.criticalityCondition.edit.isDescriptionVisible = !this.criticalityCondition.edit.isDescriptionVisible;
        },
        isDescriptionEmpty() {
            return !this.criticalityCondition.edit.description || this.criticalityCondition.edit.description.trim().length === 0;
        },
        isDescriptionVisible() {
            return this.criticalityCondition.edit.isDescriptionVisible || !isEmpty(this.criticalityCondition.edit.description)
        },
        save() {
            updateCriticalityCondition(this.questionnaire.questionnaireId, this.criticalityCondition.edit);
        },
        cancel() {
            var clonned = cloneDeep(this.criticalityCondition);
            clonned.edit = null;
            this.criticalityCondition.edit = clonned;
        },
    },
}
</script>

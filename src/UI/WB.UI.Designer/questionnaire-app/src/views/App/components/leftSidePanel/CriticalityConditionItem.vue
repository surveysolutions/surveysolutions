<template>
    <li class="macros-panel-item">
        <a href="javascript:void(0);" @click="remove()" v-if="!isReadOnlyForUser" class="btn delete-btn"
            tabindex="-1"></a>
        <ExpressionEditor v-model="criticalityCondition.edit.expression" mode="expression" focusable="false"
            :placeholder="$t('QuestionnaireEditor.SideBarCriticalityConditionExpression')" />
        <div class="divider"></div>
        <div class="input-group macros-name">
            <input :id="criticalityCondition.id"
                :placeholder="$t('QuestionnaireEditor.SideBarCriticalityConditionMessage')" maxlength="32"
                spellcheck="false" v-model="criticalityCondition.edit.message" name="message" class="form-control"
                type="text" />
        </div>
        <div v-if="isDescriptionVisible()">
            <div class="divider"></div>
            <textarea :placeholder="$t('QuestionnaireEditor.SideBarCriticalityConditionDescription')" type="text"
                v-model="criticalityCondition.edit.description" class="form-control macros-description"
                v-autosize></textarea>
        </div>
        <div class="actions" v-if="isDirty">
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
    </li>
</template>

<script>
import ExpressionEditor from '../ExpressionEditor.vue';
import { isEmpty, cloneDeep } from 'lodash'
import { updateCriticalityCondition, deleteCriticalityCondition } from '../../../../services/criticalityConditionsService';
import { createQuestionForDeleteConfirmationPopup } from '../../../../services/utilityService'

export default {
    name: 'CriticalityConditionItem',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    components: {
        ExpressionEditor,
    },
    props: {
        questionnaireId: { type: String, required: true },
        criticalityCondition: { type: Object, required: true },
    },
    data() {
        return {}
    },
    computed: {
        isDirty() {
            return this.criticalityCondition.message !== this.criticalityCondition.edit.message
                || this.criticalityCondition.expression !== this.criticalityCondition.edit.expression
                || this.criticalityCondition.description !== this.criticalityCondition.edit.description;
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

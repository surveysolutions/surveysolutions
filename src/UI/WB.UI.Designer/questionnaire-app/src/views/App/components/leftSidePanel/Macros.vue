<template>
    <div class="macroses">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>
                    {{ $t('QuestionnaireEditor.SideBarMacroCounter', {
                        count: macros.length,
                    }) }}
                </span>
            </h3>
            <div class="empty-list" v-if="macros.length == 0">
                <p>{{ $t('QuestionnaireEditor.SideBarMacroEmptyLine1') }} </p>
                <p>{{ $t('QuestionnaireEditor.SideBarMacroEmptyLine2') }}</p>
                <p>
                    <span class="variable-name">{{ $t('QuestionnaireEditor.VariableName') }}</span>
                    {{ $t('QuestionnaireEditor.SideBarMacroEmptyLine3') }}
                </p>
            </div>
            <ul>
                <li class="macros-panel-item" v-for="macro in macros">
                    <a href="javascript:void(0);" @click="removeMacro(macro)" v-if="!questionnaire.isReadOnlyForUser"
                        class="btn delete-btn" tabindex="-1"></a>
                    <div class="input-group macros-name">
                        <span class="input-group-addon">$</span>
                        <input :placeholder="$t('QuestionnaireEditor.SideBarMacroName')" maxlength="32" spellcheck="false"
                            v-model="macro.name" name="name" class="form-control" type="text" />
                    </div>
                    <div class="divider"></div>
                    <ExpressionEditor v-model="macro.content" mode="expression" focusable="false" />
                    <div v-if="isDescriptionVisible(macro)">
                        <div class="divider"></div>
                        <textarea :placeholder="$t('QuestionnaireEditor.SideBarMacroDescription')" type="text"
                            v-model="macro.description" class="form-control macros-description" v-autosize></textarea>
                    </div>
                    <div class="actions" v-if="isEditorDirty(macro)">
                        <button :disabled="questionnaire.isReadOnlyForUser" class="btn lighter-hover"
                            @click="saveMacro(macro)">{{ $t('QuestionnaireEditor.Save') }}
                        </button>
                        <button type="button" class="btn lighter-hover" @click="cancel(macro)">{{
                            $t('QuestionnaireEditor.Cancel') }}</button>
                        <button class="btn btn-default pull-right" v-if="isDescriptionEmpty(macro)" type="button"
                            @click="toggleDescription(macro)">
                            {{ isDescriptionVisible(macro) ? $t('QuestionnaireEditor.SideBarMacroHideDescription') :
                                $t('QuestionnaireEditor.SideBarMacroShowDescription') }}
                        </button>
                    </div>
                </li>
            </ul>
            <div class="button-holder">
                <input type="button" class="btn lighter-hover" :disabled="questionnaire.isReadOnlyForUser"
                    :value="$t('QuestionnaireEditor.SideBarAddMacro')" @click="addNewMacro()">
            </div>
        </perfect-scrollbar>
    </div>
</template>
  
<script>
import { useQuestionnaireStore } from '../../../../stores/questionnaire';
import ExpressionEditor from '../ExpressionEditor.vue';
import { createQuestionForDeleteConfirmationPopup } from '../../../../services/utilityService'
import _ from 'lodash';
import { newGuid } from '../../../../helpers/guid';
import { addMacro, deleteMacro, updateMacro } from '../../../../services/macrosService';

export default {
    name: 'Macroses',
    props: {
        questionnaireId: { type: String, required: true },
    },
    components: {
        ExpressionEditor,
    },
    data() {
        return {}
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();

        return {
            questionnaireStore
        };
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.info || {};
        },
        macros() {
            return this.questionnaire.macros || [];
        },
    },
    methods: {
        addNewMacro() {
            const id = newGuid();
            addMacro(this.questionnaire.questionnaireId, id);
        },
        removeMacro(macro) {
            const params = createQuestionForDeleteConfirmationPopup(
                macro.name || this.$t('QuestionnaireEditor.SideBarMacroNoName')
            );

            params.callback = confirm => {
                if (confirm) {
                    deleteMacro(this.questionnaire.questionnaireId, macro.itemId);
                }
            };

            this.$confirm(params);

        },
        toggleDescription(macro) {
            macro.isDescriptionVisible = !macro.isDescriptionVisible;
        },
        isDescriptionEmpty(macro) {
            return macro.description.trim().length === 0;
        },
        isDescriptionVisible(macro) {
            return macro.isDescriptionVisible || !_.isEmpty(macro.description)
        },
        saveMacro(macro) {
            updateMacro(this.questionnaire.questionnaireId, macro);
        },
        cancel(macro) {
            Object.assign(macro, macro.initialMacro);
        },
        isEditorDirty(macro) {
            return macro.name !== macro.initialMacro.name || macro.content !== macro.initialMacro.content || macro.description !== macro.initialMacro.description;
        }
    }
}
</script>
  
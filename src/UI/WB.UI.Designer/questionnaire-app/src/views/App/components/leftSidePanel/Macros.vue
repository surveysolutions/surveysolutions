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
                    <a href="javascript:void(0);" @click="removeMacro(macro)" v-if="!isReadOnlyForUser"
                        class="btn delete-btn" tabindex="-1"></a>
                    <div class="input-group macros-name">
                        <span class="input-group-addon">$</span>
                        <input :id="macro.itemId" :placeholder="$t('QuestionnaireEditor.SideBarMacroName')"
                            maxlength="32" spellcheck="false" v-model="macro.editMacro.name" name="name"
                            class="form-control" type="text" />
                    </div>
                    <div class="divider"></div>
                    <div class="macro-editor">
                        <ExpressionEditor v-model="macro.editMacro.content" mode="expression" focusable="false" />
                    </div>
                    <div v-if="isDescriptionVisible(macro)">
                        <div class="divider"></div>
                        <textarea :placeholder="$t('QuestionnaireEditor.SideBarMacroDescription')" type="text"
                            v-model="macro.editMacro.description" class="form-control macros-description"
                            v-autosize></textarea>
                    </div>
                    <div class="actions" v-if="isEditorDirty(macro)">
                        <button v-if="!isReadOnlyForUser" class="btn lighter-hover" @click="saveMacro(macro)">{{
                        $t('QuestionnaireEditor.Save') }}
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
                <input type="button" class="btn lighter-hover" v-if="!isReadOnlyForUser"
                    :value="$t('QuestionnaireEditor.SideBarAddMacro')" @click="addNewMacro()">
            </div>
        </perfect-scrollbar>
    </div>
</template>

<script>
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
    inject: ['questionnaire', 'isReadOnlyForUser'],
    components: {
        ExpressionEditor,
    },
    data() {
        return {}
    },
    computed: {
        macros() {
            return this.questionnaire.macros;
        },
    },
    methods: {
        addNewMacro() {
            const id = newGuid();
            addMacro(this.questionnaire.questionnaireId, id);
        },
        removeMacro(macro) {
            const params = createQuestionForDeleteConfirmationPopup(
                macro.editMacro.name || this.$t('QuestionnaireEditor.SideBarMacroNoName')
            );

            params.callback = confirm => {
                if (confirm) {
                    deleteMacro(this.questionnaire.questionnaireId, macro.itemId);
                }
            };

            this.$confirm(params);

        },
        toggleDescription(macro) {
            macro.editMacro.isDescriptionVisible = !macro.editMacro.isDescriptionVisible;
        },
        isDescriptionEmpty(macro) {
            return macro.editMacro.description.trim().length === 0;
        },
        isDescriptionVisible(macro) {
            return macro.editMacro.isDescriptionVisible || !_.isEmpty(macro.editMacro.description)
        },
        saveMacro(macro) {
            updateMacro(this.questionnaire.questionnaireId, macro.editMacro);
        },
        cancel(macro) {
            var clonned = _.cloneDeep(macro);
            clonned.editMacro = null;
            macro.editMacro = clonned;
        },
        isEditorDirty(macro) {
            return macro.name !== macro.editMacro.name || macro.content !== macro.editMacro.content || macro.description !== macro.editMacro.description;
        }
    }
}
</script>
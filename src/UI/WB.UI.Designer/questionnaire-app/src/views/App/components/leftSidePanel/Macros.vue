<template>
    <div style="color: brown; font-size: large;">Under construction</div>

    <div class="macroses">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>

                    {{ $t('QuestionnaireEditor.SideBarMacroCounter', {
                        count: macros.length,
                    }) }}
                </span>
            </h3>
            <div class="empty-list" ng-show="macros.length == 0">
                <p>{{ $t('QuestionnaireEditor.SideBarMacroEmptyLine1') }} </p>
                <p>{{ $t('QuestionnaireEditor.SideBarMacroEmptyLine2') }}</p>
                <p>
                    <span class="variable-name">{{ $t('QuestionnaireEditor.VariableName') }}</span>
                    {{ $t('QuestionnaireEditor.SideBarMacroEmptyLine3') }}
                </p>
            </div>
            <form role="form" name="macrosForm" novalidate>
                <ul>
                    <li class="macros-panel-item" v-for="macro in macros">
                        <form name="macro.form">
                            <a href @click="deleteMacro($index)" :disabled="questionnaire.isReadOnlyForUser"
                                v-if="!questionnaire.isReadOnlyForUser" class="btn delete-btn" tabindex="-1"></a>
                            <div class="input-group macros-name">
                                <span class="input-group-addon">$</span>
                                <input focus-on-out="focusMacro{{macro.itemId}}"
                                    :placeholder="$t('QuestionnaireEditor.SideBarMacroName')"
                                    maxlength="32" spellcheck="false" v-model="macro.name" name="name" class="form-control"
                                    type="text" />
                            </div>
                            <div class="divider"></div>
                            <ExpressionEditor v-model="macro.content" mode="expression" />
                            <div v-if="macro.isDescriptionVisible">
                                <div class="divider"></div>
                                <textarea 
                                    :placeholder="$t('QuestionnaireEditor.SideBarMacroDescription')"    type="text"
                                    v-model="macro.description" class="form-control macros-description"
                                    msd-elastic></textarea>
                            </div>
                            <div class="actions" > <!--v-if="macro.form.$dirty" -->

                                <!-- :disabled="questionnaire.isReadOnlyForUser || macro.form.$invalid" -->

                                <button type="submit" :disabled="questionnaire.isReadOnlyForUser"
                                    class="btn lighter-hover" @click="saveMacro(macro)">{{ $t('QuestionnaireEditor.Save')
                                    }}</button>
                                <button type="button" class="btn lighter-hover" @click="cancel(macro)">{{
                                    $t('QuestionnaireEditor.Cancel') }}</button>
                                <button class="btn btn-default pull-right" v-if="isDescriptionEmpty(macro)" type="button"
                                    @click="toggleDescription(macro)"
                                    ng-i18next="{{macro.isDescriptionVisible ? 'SideBarMacroHideDescription' : 'SideBarMacroShowDescription'}}"></button>
                            </div>
                        </form>
                    </li>
                </ul>
            </form>
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
            // this.macros.push({
            //     name: '',
            //     content: '',
            //     description: '',
            //     isDescriptionVisible: false,
            //     itemId: this.questionnaire.generateItemId(),
            //     form: {},
            // });
        },
        deleteMacro(index) {
            // this.macros.splice(index, 1);
        },
        toggleDescription(macro) {
            // macro.isDescriptionVisible = !macro.isDescriptionVisible;
        },
        isDescriptionEmpty(macro) {
            // return macro.description.trim().length === 0;
        },

    }
}
</script>
  
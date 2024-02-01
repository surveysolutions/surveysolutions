<template>
    <form name="scenario.form">
        <a href="javascript:void(0)" v-if="!isReadOnlyForUser" @click="deleteScenario()" class="btn delete-btn"
            tabindex="-1"></a>
        <div class="input-group macroses-name">
            <input focus-on-out="focusScenario{{scenario.id}}" :placeholder='$t("QuestionnaireEditor.SideBarScenarioName")'
                maxlength="32" spellcheck="false" v-model="scenario.title" name="name" class="form-control" type="text" />
        </div>
        <div class="divider"></div>
        <button type="button" class="btn lighter-hover" @click="runScenario()">{{
            $t('QuestionnaireEditor.Run') }}</button>
        <ScenarioEditor ref="editor" :scenario="scenario" :questionnaire-id="questionnaireId"></ScenarioEditor>
        <button type="button" class="btn lighter-hover" @click="showScenarioEditor()">{{
            $t('QuestionnaireEditor.View')
        }}</button>
        <div class="actions" v-show="dirty" v-if="!isReadOnlyForUser">
            <button type="button" :disabled="isReadOnlyForUser || invalid" class="btn lighter-hover"
                @click="saveScenario()">{{
                    $t('QuestionnaireEditor.Save') }}</button>
            <button type="button" class="btn lighter-hover" @click="cancel()">{{
                $t('QuestionnaireEditor.Cancel') }}</button>
        </div>
    </form>
</template>

<script>

import { createQuestionForDeleteConfirmationPopup } from '../../../../services/utilityService'
import { deleteScenario, updateScenario, runScenario } from '../../../../services/scenariosService'
import ScenarioEditor from './ScenarioEditor.vue';

export default {
    name: 'ScenarioItem',
    components: { ScenarioEditor },
    inject: ['isReadOnlyForUser'],
    props: {
        questionnaireId: { type: String, required: true },
        scenario: { type: Object, required: true },
    },
    data() {
        return {
            originName: null,
        }
    },
    beforeMount() {
        this.originName = this.scenario.title
    },
    computed: {
        dirty() {
            return this.originName != this.scenario.title;
        },
        invalid() {
            return false;
        }
    },
    methods: {
        runScenario() {
            runScenario(this.questionnaireId, this.scenario.id)
        },
        async showScenarioEditor() {
            await this.$refs.editor.show();
        },
        saveScenario() {
            updateScenario(this.questionnaireId, this.scenario)
        },
        cancel() {
            this.scenario.title = this.originName;
        },
        deleteScenario() {
            var scenarioName = this.scenario.title || $t("QuestionnaireEditor.SideBarScenarioNoName");
            var modalInstance = createQuestionForDeleteConfirmationPopup(scenarioName);

            modalInstance.callback = confirm => {
                if (confirm) {
                    deleteScenario(this.questionnaireId, this.scenario.id)
                }
            };

            this.$confirm(modalInstance);
        },
    }
}
</script>

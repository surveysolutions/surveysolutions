<template>
    <div class="macroses">
        <perfect-scrollbar class="scroller">
            <h3>
                <span>{{ $t('QuestionnaireEditor.SideBarScenarioCounter', { count: scenarios.length }) }}</span>
            </h3>
            <div class="empty-list" v-show="scenarios.length == 0">
                <p>{{ $t('QuestionnaireEditor.SideBarScenarioEmptyLine1') }}</p>
                <p>{{ $t('QuestionnaireEditor.SideBarScenarioEmptyLine2') }}</p>
            </div>
            <form role="form" name="scenariosForm" novalidate>
                <ul>
                    <li class="macroses-panel-item" v-for="scenario in scenarios">
                        <ScenarioItem :scenario="scenario" :questionnaire-id="questionnaireId"></ScenarioItem>
                    </li>
                </ul>
            </form>
        </perfect-scrollbar>
    </div>
</template>
  
<script>

import ScenarioItem from './ScenarioItem.vue';
import { useQuestionnaireStore } from '../../../../stores/questionnaire';

export default {
    name: 'Scenarios',
    components: { ScenarioItem, },
    props: {
        questionnaireId: { type: String, required: true },
    },
    data() {
        return {

        }
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();

        return {
            questionnaireStore,
        };
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.getInfo;
        },

        scenarios() {
            return this.questionnaireStore.getEdittingScenarios;
        },
    }
}
</script>
  
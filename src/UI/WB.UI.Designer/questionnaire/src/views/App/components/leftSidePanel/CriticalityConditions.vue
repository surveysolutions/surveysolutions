<template>
    <div class="macroses">
        <perfect-scrollbar class="scroller">
            <h3>{{ $t('QuestionnaireEditor.SideBarCriticalityConditionsCounter', {
                count: criticalityConditions.length
            }) }}
            </h3>

            <div class="empty-list" v-show="criticalityConditions.length == 0">
                <p>{{ $t('QuestionnaireEditor.SideBarCriticalityConditionsEmptyLine1') }}</p>
                <p>{{ $t('QuestionnaireEditor.SideBarCriticalityConditionsEmptyLine2') }}</p>
                <p>{{ $t('QuestionnaireEditor.SideBarCriticalityConditionsEmptyLine3') }}</p>
            </div>

            <ul>
                <li class="macros-panel-item" style="padding:0px;" aria-label="CriticalRule"
                    v-for="(criticalityCondition, index) in criticalityConditions">
                    <CriticalityConditionItem :criticalityCondition="criticalityCondition" :index="index"
                        :questionnaire-id="questionnaireId" />
                </li>
            </ul>
            <div class="button-holder">
                <input type="button" class="btn lighter-hover" v-if="!isReadOnlyForUser"
                    :value="$t('QuestionnaireEditor.SideBarAddCriticalityCondition')"
                    @click="addNewCriticalityConditions()">
            </div>

        </perfect-scrollbar>
    </div>
</template>

<script>

import CriticalityConditionItem from './CriticalityConditionItem.vue';

import { newGuid } from '../../../../helpers/guid';
import { addCriticalityCondition } from '../../../../services/criticalityConditionsService'

export default {
    name: 'CriticalityConditions',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    components: { CriticalityConditionItem, },
    props: {
        questionnaireId: { type: String, required: true },
    },
    data() {
        return {}
    },
    computed: {
        criticalityConditions() {
            return this.questionnaire.criticalityConditions;
        },
    },
    methods: {
        async addNewCriticalityConditions() {
            const id = newGuid();

            await addCriticalityCondition(this.questionnaireId, id)
        },
    },
}
</script>

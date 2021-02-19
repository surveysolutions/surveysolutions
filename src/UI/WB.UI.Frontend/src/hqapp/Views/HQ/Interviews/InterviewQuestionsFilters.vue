<template>
    <div class="filters-container"
        id="questionsFilters">
        <h4>
            {{$t("Interviews.FiltersByQuestions")}}
        </h4>
        <div class="block-filter">
            <button type="button"
                id="btnQuestionsFilter"
                class="btn"
                :disabled="isDisabled"
                :title="isDisabled ? $t('Interviews.QuestionsFilterNotAvailable'):''"
                @click="$refs.questionsSelector.modal()">
                {{$t("Interviews.QuestionsSelector")}}
            </button>
        </div>

        <ModalFrame ref="questionsSelector"
            id="modalQuestionsSelector"
            :title="$t('Interviews.ChooseQuestionsTitle')">
            <form onsubmit="return false;">
                <div class="action-container">
                    <!-- <div class="pull-right">
                        <span>There will be language selector</span>
                    </div> -->
                    <div>
                        <Checkbox v-for="questionnaireItem in questionnaireItemsList"
                            :key="'cb_' + questionnaireItem.variable"
                            :label="`${sanitizeHtml(questionnaireItem.title)}`"
                            :value="isChecked(questionnaireItem)"
                            :name="'check_' + questionnaireItem.variable"
                            @input="check(questionnaireItem)" />
                    </div>
                </div>
            </form>
            <div slot="actions">
                <button
                    id="btnQuestionsSelectorOk"
                    type="button"
                    class="btn btn-primary"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Ok") }}</button>
            </div>
        </ModalFrame>

        <ModalFrame ref="questionsExposedSelector"
            id="modalQuestionsExposedSelector"
            :title="$t('Interviews.DynamicFilter')">
            <vue-query-builder
                :rules="rules"
                :maxDepth="5"
                :labels="labels"
                v-model="queryExposedVariables"></vue-query-builder>

            <div slot="actions">
                <button
                    id="btnQuestionsExposedSelectorOk"
                    type="button"
                    class="btn btn-primary"
                    @click="saveExposedVariablesFilter">{{ $t("Common.Save") }}</button>
            </div>
        </ModalFrame>

        <InterviewFilter

            v-for="condition in conditions"
            :key="'filter_' + condition.variable"
            :id="'filter_' + condition.variable"
            :item="itemFor(condition)"
            :condition="condition"
            @change="conditionChanged">
        </InterviewFilter>

        <div class="block-filter">
            <button type="button"
                id="btnExposedQuestionsFilter"
                class="btn"
                :disabled="isDynamicDisabled"
                :title="isDynamicDisabled ? $t('Interviews.DynamicFilterNotAvailable'):''"
                @click="$refs.questionsExposedSelector.modal()">
                {{$t("Interviews.AdvancedFilterSelector")}}
            </button>
        </div>

    </div>

</template>
<script>

import VueQueryBuilder from 'vue-query-builder'
import 'vue-query-builder/dist/VueQueryBuilder.css'

import gql from 'graphql-tag'
import InterviewFilter from './InterviewFilter'
import { find, filter } from 'lodash'
import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text,  { allowedTags: [], allowedAttributes: [] })

export default {
    data() {
        return {
            queryExposedVariables:{},
            conditions: [], /** { } */
            questionnaireItems: [],
            selectedQuestion: null,
            checked: {},

            labels:{
                'matchType': 'Match Type',
                'matchTypes': [
                    {'id': 'all', 'label': 'AND'},
                    {'id': 'any', 'label': 'OR'},
                ],
                'addRule': 'Add Rule',
                'removeRule': '&times;',
                'addGroup': 'Add Group',
                'removeGroup': '&times;',
                'textInputPlaceholder': 'value',
            },
        }
    },

    props: {
        questionnaireId: {
            type: String, required: false,
        },
        questionnaireVersion : {
            type: Number,
        },
        questionsFilter:{ type: Object},
        value: {type: Array},
        exposedValuesFilter: {type: Object},
    },

    apollo: {
        questionnaireItems:{
            query :gql`query questionnaireItems($workspace: String!, $id: Uuid!, $version: Long!) {
                questionnaireItems(workspace: $workspace, id: $id, version: $version, where: { or: [{identifying: {eq: true}} , {exposed: {eq: true}}]}) {
                    title, type, variable, entityType, identifying
                    options { title, value, parentValue }
                }
            }`,
            variables() {
                return {
                    id: (this.questionnaireId || '').replace(/-/g, ''),
                    version: this.questionnaireVersion,
                    workspace: this.$store.getters.workspace,
                }
            },
            skip() {
                return this.questionnaireId == null || this.questionnaireVersion == null
            },
        },
    },

    mounted() {
        if(this.value != null) {
            this.conditions = this.value
        }
    },

    watch: {
        value(to) {
            this.conditions = this.value
        },

        questionnaireId() {
            this.conditions = this.value
        },

        questionnaireVersion() {
            this.conditions = this.value
        },
    },

    methods: {
        isChecked(item){
            return find(this.conditions, {variable: item.variable}) != null
        },

        check(item) {
            const condition = find(this.conditions, {variable: item.variable})

            if(condition == null) {
                this.conditions.push({variable: item.variable, field: null, value: null})
            } else {
                this.conditions = filter(this.conditions, c => c.variable != item.variable)
            }

            this.$emit('change', [...this.conditions])
        },

        itemFor(condition) {
            const entities = find(this.questionnaireItems, { variable: condition.variable })
            return entities
        },

        getTypeaheadValues(options) {
            return options.map(o => {
                return {
                    key: o.value,
                    value: o.title,
                }
            })
        },

        conditionChanged(changedCondition) {
            const condition = find(this.conditions, {variable: changedCondition.variable})

            if(condition == null) {
                this.conditions.push(changedCondition)
            }

            if(condition != null) {
                condition.field = changedCondition.field
                condition.value = changedCondition.value
                this.$emit('change', [...this.conditions])
            }
        },
        saveExposedVariablesFilter(){
            //this.exposedValuesFilter = this.transformQuery
            this.$emit('changeFilter', this.transformQuery)
            this.$refs.questionsExposedSelector.hide()
        },

        sanitizeHtml: sanitizeHtml,

        handleGroup(group){
            var result = {}

            if(group.logicalOperator == 'any')
            {
                result.or = []
                group.children.forEach(element => {
                    if(element.type == 'query-builder-rule')
                        result.or.push(this.handleRule(element))
                    else if(element.type == 'query-builder-group')
                    {
                        result.or.push(this.handleGroup(element))
                    }
                })
            }
            else if (group.logicalOperator == 'all')
            {
                result.and = []
                group.children.forEach(element => {
                    if(element.type == 'query-builder-rule')
                        result.and.push(this.handleRule(element))
                    else if(element.type == 'query-builder-group')
                    {
                        result.and.push(this.handleGroup(element))
                    }
                })
            }

            return result
        },
        handleRule(rule){
            var result = {
                reportAnswers :
                {
                    some:
                    {
                        entity: {variable: {eq: rule.query.operand}},
                        value: {eq: rule.query.value},
                        isEnabled: {eq: true}}}}

            return result
        },
    },

    computed: {
        questionnaireItemsList() {
            const array = filter([...(this.questionnaireItems || [])], q => {
                return (q.type == 'SINGLEOPTION'
                    || q.type == 'TEXT'
                    || q.type == 'NUMERIC'
                    || q.entityType == 'VARIABLE')
                    && q.identifying
            })

            return array
        },

        isDisabled() {
            return this.questionnaireId == null
                || this.questionnaireVersion == null
                || this.questionnaireItemsList == null
                || this.questionnaireItemsList.length == 0
        },
        isDynamicDisabled() {
            return this.questionnaireId == null
                || this.questionnaireVersion == null
                || this.questionnaireItemsList == null
                || this.rules.length == 0
        },

        rules(){

            return this.questionnaireItems.filter(i=>!i.identifying).map(i => {

                var type = 'text'

                if(i.entityType == 'VARIABLE')
                    type = 'text'
                if (i.type == 'NUMERIC')
                    type = 'numeric'
                else if(i.type == 'SINGLEOPTION')
                    type = 'select'
                else
                    type = 'text'

                var rule = {
                    type: type,
                    id:i.variable,
                    label: i.entityType == 'VARIABLE' ? i.variable :  sanitizeHtml(i.title),
                }

                if(type == 'select')
                {
                    rule.choices = i.options.map(o=>({label:o.title, value:o.value}))
                }

                return rule
            })

        },
        transformQuery(){
            return this.handleGroup(this.queryExposedVariables)
        },
    },

    components: {
        InterviewFilter,
        VueQueryBuilder,
    },
}
</script>
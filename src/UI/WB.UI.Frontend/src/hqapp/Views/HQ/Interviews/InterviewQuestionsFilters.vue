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
                :maxDepth="6"
                :labels="labels"
                v-model="queryExposedVariables">
                <template v-slot:default="slotProps">
                    <query-builder-group
                        v-bind="slotProps"
                        :query.sync="queryExposedVariables"/>
                </template>
            </vue-query-builder>
            <!-- <div>{{queryExposedVariables}}</div> -->
            <div slot="actions">
                <button
                    id="btnQuestionsExposedSelectorOk"
                    type="button"
                    class="btn btn-primary"
                    :disabled="saveDisabled"
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
import QueryBuilderGroup from './components/CustomBootstrapGroup.vue'
import moment from 'moment'
import { DateFormats } from '~/shared/helpers'
import gql from 'graphql-tag'
import InterviewFilter from './InterviewFilter'
import { find, filter } from 'lodash'
import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text,  { allowedTags: [], allowedAttributes: [] })

export default {
    data() {
        return {
            queryExposedVariables: { logicalOperator : 'all', children : [] },
            conditions: [], /** { } */
            questionnaireItems: [],
            selectedQuestion: null,
            checked: {},

            lastSavedQuery: null,
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
                questionnaireItems(workspace: $workspace, id: $id, version: $version, where: { or: [{identifying: {eq: true}}, {exposed: {eq: true}}]}) {
                    title, label, type, variable, entityType, variableType, identifying
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
            this.queryExposedVariables = { logicalOperator : 'all', children : [] }
            this.saveExposedVariablesFilter()
        },

        questionnaireVersion() {
            this.conditions = this.value
            this.queryExposedVariables = { logicalOperator : 'all', children : [] }
            this.saveExposedVariablesFilter()
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
            this.lastSavedQuery = this.transformQuery
            this.$emit('changeFilter', this.transformQuery)
            this.$refs.questionsExposedSelector.hide()
        },

        sanitizeHtml: sanitizeHtml,

        handleGroup(group){

            if(group.children === undefined || group.children.length == 0){
                return null
            }

            var conditions = []
            group.children.forEach(element => {
                if(element.type == 'query-builder-rule')
                    conditions.push(this.handleRule(element.query))
                else if(element.type == 'query-builder-group')
                {
                    var group = this.handleGroup(element.query)
                    if(group != null)
                        conditions.push(group)
                }
            })

            if(conditions.length == 0)
                return null

            var result = {}
            if(group.logicalOperator == 'any')
            {
                result.or = conditions
            }
            else if (group.logicalOperator == 'all')
            {
                result.and = conditions
            }

            return result

        },
        handleRule(query){

            var some = {
                entity: {variable: {eq: query.rule}},
                isEnabled: {eq: true},
            }

            var entity = this.questionnaireItems.find(i =>  i.variable === query.rule)
            var ruleMap = this.getRuleMap(entity)
            var operator = this.getOperatorMap(query.operator)

            if(operator === 'notanswered')
            {
                some.value = {eq : ''}
                var notAnsweredResult = {}
                notAnsweredResult.or =
                        [{identifyingData : { none: { entity: {variable: {eq: query.rule}}} }},
                            {identifyingData : { some: some }}]
                return notAnsweredResult
            }

            var singleCondition = {}
            if(operator === 'answered')
            {
                singleCondition['neq'] = ruleMap.ruleType == 'text' ? '' : null
            }
            else if(ruleMap.ruleType === 'select' && entity.variableType === 'BOOLEAN'){
                singleCondition[operator] = query.value == '1' ? true : false
            }
            else if(ruleMap.ruleType == 'numeric')
            {
                singleCondition[operator] = query.value ? Number(query.value) : null
            }
            else if(ruleMap.ruleType == 'date')
            {
                if(!query.value)
                {
                    singleCondition['eq'] = null
                }
                else
                    switch (operator) {
                        case 'on': {
                            var leftOn = {...some}
                            leftOn[ruleMap.valueName] = {gte : moment(query.value).format(DateFormats.date) + 'T00:00:00Z' }
                            var rightOn = {...some}
                            rightOn[ruleMap.valueName] = {lte : moment(query.value).format(DateFormats.date) + 'T23:59:59Z' }

                            var dateOnResult = {
                                and : [
                                    {identifyingData : { some: leftOn }},
                                    {identifyingData : { some: rightOn }},
                                ]}
                            return dateOnResult
                        }
                        case 'noton': {

                            var leftNotOn = {...some}
                            leftNotOn[ruleMap.valueName] = {lt : moment(query.value).format(DateFormats.date) + 'T00:00:00Z' }
                            var rightNotOn = {...some}
                            rightNotOn[ruleMap.valueName] = {gt: moment(query.value).format(DateFormats.date) + 'T23:59:59Z' }

                            var dateNotOnResult = {
                                or : [
                                    {identifyingData : { some: leftNotOn }},
                                    {identifyingData : { some: rightNotOn }},
                                ]}
                            return dateNotOnResult
                        }
                        case 'before':{
                            singleCondition['lt'] = moment(query.value).format(DateFormats.date) + 'T00:00:00Z'
                            break
                        }
                        case 'notlaterthan': {
                            singleCondition['lte'] = moment(query.value).format(DateFormats.date) + 'T23:59:59Z'
                            break
                        }
                        case 'after': {
                            singleCondition['gt'] = moment(query.value).format(DateFormats.date) + 'T23:59:59Z'
                            break
                        }
                        case 'onorafter':{
                            singleCondition['gte'] = moment(query.value).format(DateFormats.date) + 'T00:00:00Z'
                            break
                        }
                    }
            }
            else{
                singleCondition[operator] = query.value
            }

            some[ruleMap.valueName] = singleCondition

            var result = {identifyingData : { some: some }}

            return result
        },
        getRuleMap(entity){

            const comparableOperators = ['=','<>','<','<=','>','>=']
            const textOperators = ['equals','not equals','contains','not contains','starts with','not starts with']
            const selectOperators = ['equals','not equals']
            const dateOperators = ['on', 'not on', 'before', 'not later than', 'after', 'on or after']
            const unaryOperators = ['answered', 'not answered']

            if(entity.entityType =='QUESTION')
            {
                switch(entity.type) {
                    case 'NUMERIC':
                        return {ruleType: 'numeric', valueName: 'valueLong', operators: comparableOperators.concat(unaryOperators), unaryOperators: unaryOperators}
                    case 'SINGLEOPTION':
                        return {ruleType: 'select', valueName: 'answerCode', operators: selectOperators.concat(unaryOperators), unaryOperators: unaryOperators}
                    case 'DATETIME':
                        return {ruleType: 'date', valueName: 'valueDate', operators: dateOperators.concat(unaryOperators), unaryOperators: unaryOperators }
                    case 'TEXT':
                        return {ruleType:'text', valueName: 'value' , operators: textOperators.concat(unaryOperators), unaryOperators: unaryOperators}
                }
            }
            else if(entity.entityType == 'VARIABLE')
            {
                switch(entity.variableType) {
                    case 'DOUBLE':
                        return {ruleType: 'numeric', valueName: 'valueDouble', operators: comparableOperators.concat(unaryOperators), unaryOperators: unaryOperators}
                    case 'LONGINTEGER':
                        return {ruleType: 'numeric', valueName: 'valueLong', operators: comparableOperators.concat(unaryOperators), unaryOperators: unaryOperators}
                    case 'BOOLEAN':
                        return {ruleType: 'select',valueName: 'valueBool', operators: selectOperators.concat(unaryOperators), unaryOperators: unaryOperators}
                    case 'DATETIME':
                        return {ruleType: 'date', valueName: 'valueDate', operators: dateOperators.concat(unaryOperators), unaryOperators: unaryOperators}
                    case 'STRING' :
                        return {ruleType:'text', valueName: 'value', operators: textOperators.concat(unaryOperators), unaryOperators: unaryOperators}
                }
            }
            return 'text'
        },
        //move it into ruleMap
        getOperatorMap(operator){
            switch(operator){
                case '=': return 'eq'
                case '<>': return 'neq'
                case '<': return 'lt'
                case '<=': return 'lte'
                case '>': return 'gt'
                case '>=': return 'gte'

                case 'equals': return 'eq'
                case 'not equals': return 'neq'
                case 'contains': return 'contains'
                case 'not contains': return 'ncontains'
                case 'starts with': return 'startsWith'
                case 'not starts with': return 'nstartsWith'

                case 'on': return 'on'
                case 'not on': return 'noton'
                case 'before': return 'before'
                case 'not later than': return 'notlaterthan'
                case 'after': return 'after'
                case 'on or after': return 'onorafter'

                case 'not answered' : return 'notanswered'
                case 'answered' : return 'answered'
            }
        },
        getDisplayTitle(title){
            var transformedTitle = sanitizeHtml(title).replace(/%[\w_]+%/g, '[..]')
            return transformedTitle.length  >= 57 ? transformedTitle.substring(0,54) + '...' : transformedTitle
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

        questionnaireAllItemsList() {
            const array = filter([...(this.questionnaireItems || [])], q => {
                return q.type == 'SINGLEOPTION'
                    || q.type == 'TEXT'
                    || q.type == 'NUMERIC'
                    || q.type == 'DATETIME'
                    || q.entityType == 'VARIABLE'
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
                || this.questionnaireAllItemsList == null
                || this.questionnaireAllItemsList.length == 0
                || this.rules.length == 0
        },
        rules(){
            return this.questionnaireAllItemsList.map(entity => {

                var map = this.getRuleMap(entity)
                var type = map.ruleType

                var rule = {
                    type: type,
                    id: entity.variable,
                    label: entity.label
                        ? this.getDisplayTitle(entity.label)
                        : (entity.title ? this.getDisplayTitle(entity.title) : entity.variable),
                }

                if(type === 'select')
                {
                    if(entity.entityType == 'VARIABLE' && entity.variableType === 'BOOLEAN'){
                        rule.choices = [{label: 'True', value: '1'}, {label: 'False', value: '0'}]
                    }
                    else
                        rule.choices = entity.options.map(o=>({label:this.getDisplayTitle(o.title), value: o.value}))
                }
                else if(type === 'date')
                {
                    rule.inputType = 'date'
                }

                if(map.operators !== undefined)
                    rule.operators = map.operators

                if(map.unaryOperators !== undefined)
                    rule.unaryOperators = map.unaryOperators

                return rule
            })

        },
        transformQuery(){
            return this.handleGroup(this.queryExposedVariables)
        },
        labels(){
            return {
                'matchType': this.$t('Interviews.DynamicFilter_MatchType'),
                'matchTypes': [
                    {'id': 'all', 'label': this.$t('Interviews.DynamicFilter_MatchTypes_And')},
                    {'id': 'any', 'label': this.$t('Interviews.DynamicFilter_MatchTypes_Or')},
                ],
                'addRule': this.$t('Interviews.DynamicFilter_MatchTypes_AddRule'),
                'removeRule': '&times;',
                'addGroup': this.$t('Interviews.DynamicFilter_MatchTypes_AddGroup'),
                'removeGroup': '&times;',
                'textInputPlaceholder': this.$t('Interviews.DynamicFilter_MatchTypes_ValuePlaceholder'),
            }
        },
        saveDisabled(){
            return this.transformQuery === this.lastSavedQuery
        },
    },

    components: {
        InterviewFilter,
        VueQueryBuilder,
        QueryBuilderGroup,
    },
}
</script>
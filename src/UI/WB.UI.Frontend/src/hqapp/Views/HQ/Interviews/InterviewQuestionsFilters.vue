<template>
    <div class="filters-container">
        <h4>Filter by Questions</h4>
    
        <InterviewFilter 
            v-for="condition in conditionsList"
            :questions="questionsList"            
            :key="condition.variable || '__new'"
            :condition="condition"
            @remove="conditionRemoved"
            @change="conditionChanged"
            class="block-filter">
        </InterviewFilter>      
    </div>
    
</template>
<script>

import gql from 'graphql-tag'
import InterviewFilter from './InterviewFilter'
import { find, filter } from 'lodash'

export default {
    data() {
        return {
            conditions: [],
            questions: null,
            selectedQuestion: null,
        }
    },

    props: {
        questionnaireId: {
            type: String, required: true,
        },
        questionnaireVersion : {
            type: Number,
        },
        questionsFilter:{ type: Object},
    },

    apollo: {
        questions:{
            query :gql`query questions($id: Uuid, $version: Long) {
                questions(id: $id, version: $version, where: {identifying: true}) {
                    questionText, type, variable
                    options { title, value, parentValue }
                }
            }`,
            variables() {
                console.log('apollo.questions.id', (this.questionnaireId || '').replace(/-/g, ''))
                return {
                    id: (this.questionnaireId || '').replace(/-/g, ''),
                    version: this.questionnaireVersion,
                }},
        },
    },

    methods: {
        addCondition() {

            this.conditions.push({})
        },

        isCategorical(question) {
            return this.selectedQuestion != null && this.selectedQuestion.type == 'SINGLEOPTION'
        },

        getTypeaheadValues(options) {
            return options.map(o => {
                return {
                    key: o.value,
                    value: o.title,
                }
            })
        },

        conditionRemoved(variable) {
            this.conditions = filter(this.conditions, c => c.variable != variable)            
        },

        conditionChanged(changedCondition) {
            const condition = find(this.conditions, {variable: changedCondition.variable})

            if(condition == null) {
                this.conditions.push(changedCondition)
            }

            if(condition != null) {
                condition.value = changedCondition.value
                this.$emit('change', filter(this.conditions, c => c.value))
            }

        },
    },

    computed: {
        questionsList() {
            return this.questions
        },

        conditionsList() {
            const conditions = [ ...this.conditions]
            
            if(find(conditions, c => !c.value) == null) {
                conditions.push({})
            }

            return conditions
        },
    },

    components: {
        InterviewFilter,
        //   QuestionCondition,
    },
}
</script>
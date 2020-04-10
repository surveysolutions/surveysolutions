<template>
    <div class="filters-container"
        v-if="questionsList != null">
        <h4>Filters by Questions</h4>
        <div class="block-filter">            
            <button type="button"
                class="btn btn-primary btn-lg btn-block"
                @click="$refs.questionsSelector.modal()">
                Choose questions
            </button>
        </div>

        <ModalFrame ref="questionsSelector">
            <form onsubmit="return false;">
                <div class="action-container">
                    <Checkbox v-for="question in questionsList"
                        :key="'cb_' + question.variable"
                        :label="`[${question.variable}] ${question.questionText}`"
                        :value="isChecked(question)"
                        :name="'check_' + question.variable"
                        @input="check(question)" />
                </div>
            </form>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Ok") }}</button>
            </div>
        </ModalFrame>
             
        <InterviewFilter 
            v-for="condition in conditions"         
            :key="'filter_' + condition.variable"
            :question="questionFor(condition)"            
            :condition="condition"
            @change="conditionChanged">
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
            conditions: [], /** { } */
            questions: null,
            selectedQuestion: null,
            checked: {},
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
                return {
                    id: (this.questionnaireId || '').replace(/-/g, ''),
                    version: this.questionnaireVersion,
                }},
        },
    },

    methods: {
        isChecked(question){
            return find(this.conditions, {variable: question.variable}) != null
        },

        check(question) {
            const condition = find(this.conditions, {variable: question.variable})
            
            if(condition == null) {
                this.conditions.push({variable: question.variable, value: null})
            } else {
                this.conditions = filter(this.conditions, c => c.variable != question.variable)
            }
        },

        questionFor(condition) {
            const question = find(this.questions, { variable: condition.variable })
            return question
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
            const array = [...(this.questions || [])]
            array.sort(function (a, b) {
                return a.questionText.localeCompare(b.questionText)
            })
            return array
        },
    },

    components: {
        InterviewFilter,
    },
}
</script>
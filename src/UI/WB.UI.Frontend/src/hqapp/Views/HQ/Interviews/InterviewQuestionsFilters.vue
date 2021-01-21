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

        <InterviewFilter

            v-for="condition in conditions"
            :key="'filter_' + condition.variable"
            :id="'filter_' + condition.variable"
            :item="itemFor(condition)"
            :condition="condition"
            @change="conditionChanged">
        </InterviewFilter>
    </div>

</template>
<script>

import gql from 'graphql-tag'
import InterviewFilter from './InterviewFilter'
import { find, filter } from 'lodash'
import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text,  { allowedTags: [], allowedAttributes: [] })

export default {
    data() {
        return {
            conditions: [], /** { } */
            questionnaireItems: null,
            selectedQuestion: null,
            checked: {},
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
    },

    apollo: {
        questionnaireItems:{
            query :gql`query questionnaireItems($workspace: String!, $id: Uuid!, $version: Long!) {
                questionnaireItems(workspace: $workspace, id: $id, version: $version, where: { identifying: {eq: true} }) {
                    title, type, variable, entityType
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

        sanitizeHtml: sanitizeHtml,
    },

    computed: {
        questionnaireItemsList() {
            const array = filter([...(this.questionnaireItems || [])], q => {
                return q.type == 'SINGLEOPTION'
                    || q.type == 'TEXT'
                    || q.type == 'NUMERIC'
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
    },

    components: {
        InterviewFilter,
    },
}
</script>
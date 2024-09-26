<template>
    <div>
        <FilterBlock :title="$t('Reports.Questionnaire')">
            <Typeahead control-id="questionnaire" :placeholder="selectedQuestionnairePlaceholder" noClear
                :values="questionnaireList" :value="selectedQuestionnaire" :forceLoadingState="loading.questionnaire"
                @selected="selectQuestionnaire" />
        </FilterBlock>

        <FilterBlock :title="$t('Common.QuestionnaireVersion')">
            <Typeahead control-id="version" :placeholder="$t('Common.AllVersions')" noSearch
                :values="questionnaireVersionsList" :value="selectedQuestionnaireVersion"
                :forceLoadingState="loading.questionnaire" :disabled="selectedQuestionnaire == null"
                @selected="selectQuestionnaireVersion" />
        </FilterBlock>

        <FilterBlock :title="$t('Common.Status')">
            <Typeahead control-id="status" :selectedKey="selectedStatus" data-vv-name="status" data-vv-as="status"
                :placeholder="$t('Common.AllStatuses')" :value="status" :values="statuses"
                v-on:selected="statusSelected" />
        </FilterBlock>

        <FilterBlock :title="$t('Reports.Question')">
            <Typeahead control-id="question" :placeholder="selectedQuestionPlaceholder" noClear
                :forceLoadingState="loading.questions" :values="questionsList" :value="selectedQuestion"
                @selected="selectQuestion" />
        </FilterBlock>

        <FilterBlock :title="$t('Reports.ViewOptions')" v-if="!isSupervisor && this.question != null">
            <div class="options-group">
                <Radio :label="$t('Reports.TeamLeadsOnly')" :radioGroup="false" name="expandTeams" :value="expandTeams"
                    @input="radioChanged" />
                <Radio :label="$t('Reports.WithInterviewers')" :radioGroup="true" name="expandTeams"
                    :value="expandTeams" @input="radioChanged" />
            </div>
        </FilterBlock>

        <FilterBlock :title="$t('Reports.ByAnswerValue')" v-if="question && question.Type == 'Numeric'">
            <Form as="div" v-slot="{ errors, meta }" class="row">
                <div class="col-xs-6">
                    <div class="form-group" v-bind:class="{ 'has-error': errors.min }" :title="errors.min">
                        <label for="min">
                            {{ $t("Reports.Min") }}
                        </label>
                        <Field type="number" class="form-control input-sm" name="min" :placeholder="$t('Reports.Min')"
                            @input="inputChange" :rules.initial="{ max_value: max || Number.MAX_VALUE }" :value="min" />
                    </div>
                </div>
                <div class="col-xs-6">
                    <div class="form-group" :class="{ 'has-error': errors.max }" :title="errors.max">
                        <label for="max">
                            {{ $t("Reports.Max") }}
                        </label>
                        <Field type="number" class="form-control input-sm" :placeholder="$t('Reports.Max')"
                            :rules.initial="{ min_value: min || Number.MIN_VALUE }" name="max" @input="inputChange"
                            :value="max" />
                    </div>
                </div>
            </Form>
        </FilterBlock>

        <template v-if="question != null && question.SupportConditions">
            <FilterBlock :title="$t('Reports.ConditionQuestion')">
                <Typeahead control-id="condition" :placeholder="$t('Reports.SelectConditionQuestion')"
                    :values="conditionVariablesList" :value="selectedCondition" @selected="selectCondition" />
            </FilterBlock>
            <template v-if="condition != null">
                <Checkbox :label="$t('Reports.PivotView')" name="pivot" :value="query.pivot" @input="pivotChanged" />

                <ul class="list-group small" v-if="!query.pivot">
                    <li class="list-group-item pointer" v-for="answer in condition.Answers" :key="answer.Answer"
                        :class="{ 'list-group-item-success': isSelectedAnswer(answer.Answer) }"
                        @click="selectConditionAnswer(answer.Answer)">{{ answer.Answer }}. {{ answer.Text }}</li>
                </ul>
            </template>
        </template>
    </div>
</template>

<script>
import routeSync from '~/shared/routeSync'
import { xor, find, assign, isEqual, chain, filter } from 'lodash'
import { Form, Field, ErrorMessage } from 'vee-validate'

export default {
    mixins: [
        routeSync,
    ],

    components: {
        Form,
        Field,
        ErrorMessage
    },
    data() {
        return {
            selectedQuestionnaire: null,
            selectedQuestionnaireVersion: null,
            questionnaires: [],
            questions: [],
            selectedAnswers: [],
            status: null,
            selectedStatus: null,
            changesQueue: [],
            statuses: this.$config.model.statuses,
            loading: {
                questions: false,
                questionnaire: false,
            },
        }
    },

    props: {
        isSupervisor: { default: false },
    },

    watch: {
        filter(value) {
            this.$emit('input', value)
        },

        selectedQuestion(to) {
            if (to == null && this.questionsList != null && this.questionsList.length > 0) {
                this.selectQuestion(this.questionsList[0])
            }
        },
    },

    async mounted() {
        await this.loadQuestionnaires()

        if (this.query.status != null) {
            this.status = find(this.statuses, { key: this.query.status })
        }

        if (this.query.name != null)
            this.selectedQuestionnaire = find(this.questionnaireList, { value: this.query.name })

        let key = parseInt(this.query.version)
        key = Number.isNaN(key) ? null : key
        this.selectedQuestionnaireVersion = find(this.questionnaireVersionsList, { key })

        if (this.selectedQuestionnaire == null && this.questionnaireList.length > 0)
            this.selectedQuestionnaire = this.questionnaireList[this.questionnaireList.length - 1]

        await this.selectQuestionnaireInt(this.selectedQuestionnaire)

        if (this.question == null && this.questionsList.length > 0) {
            this.selectQuestion(this.questionsList[0])
        }

        this.$emit('mounted')
    },

    methods: {
        async loadQuestionnaires() {
            this.loading.questionnaire = true

            try {
                this.questionnaires = await this.$hq.Report.SurveyStatistics.Questionnaires()
            } finally {
                this.loading.questionnaire = false
            }
        },

        async loadQuestions(questionnaire = null, version = null) {

            if (questionnaire == null && this.selectedQuestionnaire == null) return
            const id = questionnaire || this.selectedQuestionnaire.key

            this.loading.questions = true

            await this.$hq.Report.SurveyStatistics.Questions(id, version)
                .then(questions => {
                    this.questions = questions
                    this.loading.questions = false
                })
                .catch(() => (this.loading.questions = false))
        },

        async selectQuestionnaire(questionnaire) {

            this.selectedQuestionnaireVersion = null
            await this.selectQuestionnaireInt(questionnaire)

        },
        async selectQuestionnaireInt(questionnaire) {
            this.selectedQuestionnaire = questionnaire
            const questionnaireId = questionnaire == null ? null : questionnaire.key

            if (questionnaire == null) {
                this.selectQuestion(null)
                this.selectCondition(null)
                return
            }

            this.onChange(query => (query.name = questionnaire.value))
            await this.selectQuestionnaireVersion(this.selectedQuestionnaireVersion)
        },

        async selectQuestionnaireVersion(questionnaireVersion) {
            const version = questionnaireVersion == null ? null : questionnaireVersion.key
            if (this.selectedQuestionnaire == null) return

            this.selectedQuestionnaireVersion = questionnaireVersion
            this.onChange(query => (query.version = version))

            await this.loadQuestions(this.selectedQuestionnaire.key, version)

            this.selectCondition(null)
            const question = find(this.questionsList, 'key', this.query.questionId)
            this.selectQuestion(question)
        },

        selectQuestion(id) {
            this.onChange(query => {
                query.questionId = id == null ? null : id.name

                if (id != null && !id.SupportConditions) {
                    query.conditionId = null
                } else {
                    query.conditionId = null
                    query.expandTeams = false
                }
            })
        },

        selectCondition(id) {
            this.onChange(query => {
                query.conditionId = id == null ? null : id.name

                if (id == null) {
                    query.pivot = false
                }
            })
        },

        statusSelected(newValue) {
            this.onChange(query => {
                query.status = newValue == null ? null : newValue.key
            })

            this.status = newValue
        },

        pivotChanged(value) {
            this.onChange(query => {
                query.pivot = value
            })
        },

        isSelectedAnswer(conditionAnswerKey) {
            return find(this.selectedAnswers, a => a == conditionAnswerKey) != null
        },

        selectConditionAnswer(answer) {
            this.selectedAnswers = xor(this.selectedAnswers, [answer])

            this.onChange(query => {
                query.ans = this.selectedAnswers
            })
        },
    },

    computed: {
        filter() {
            const state = this.queryString

            const filter = assign(
                {
                    questionnaireId: this.questionnaire == null ? null : this.questionnaire.Id,
                    questionnaire: this.questionnaire,
                    version: this.version,
                    question: this.question,
                    condition: this.condition,
                    conditionAnswers: this.conditionAnswers,
                },
                state
            )
            filter.status = this.status == null ? null : JSON.parse(this.status.alias)
            return filter
        },

        queryString() {
            return {
                name: this.query.name,
                questionId: this.query.questionId,
                conditionId: this.query.conditionId,
                ans: this.condition != null ? this.selectedAnswers : null,
                expandTeams: this.expandTeams,
                pivot: this.query.pivot,
                min: this.min,
                max: this.max,
                status: this.query.status,
                version: this.query.version == '*' ? null : this.query.version,
            }
        },

        max() {
            const result = parseInt(this.query.max)
            return isNaN(result) ? null : result
        },

        expandTeams() {
            return this.query.expandTeams === true || this.query.expandTeams === 'true'
        },

        min() {
            const result = parseInt(this.query.min)
            return isNaN(result) ? null : result
        },

        questionnaireList() {
            return chain(this.questionnaires)
                .orderBy(['Title'], ['asc'])
                .map(q => {
                    return {
                        key: q.Id,
                        value: q.Title,
                    }
                })
                .uniqWith(isEqual)
                .value()
        },

        questionnaireVersionsList() {
            var val = chain(this.questionnaires)
                .filter(c => this.selectedQuestionnaire != null && this.selectedQuestionnaire.key == c.Id)
                .orderBy(['Title', 'Version'], ['asc', 'asc'])
                .map(q => {
                    return {
                        key: q.Version,
                        value: `ver. ${q.Version}`,
                    }
                })
                .uniqWith(isEqual)
                .value()

            return val
        },

        questionsList() {
            function getValue(question) {
                let result = `[${question.VariableName}]`

                if (question.label) {
                    result += ' ' + question.Label + '\r\n' + question.QuestionText
                } else {
                    result += ' ' + question.QuestionText
                }

                return result
            }

            const questions = chain(this.questions)
                .map(q => {
                    return {
                        key: q.Id,
                        name: q.VariableName,
                        supportConditions: q.SupportConditions,
                        value: getValue(q),
                        breadcrumbs: q.Breadcrumbs,
                    }
                })
                .value()
            return questions
        },

        conditionVariablesList() {
            return chain(this.questionsList)
                .filter('supportConditions', true)
                .filter(q => q.key != this.selectedQuestion.key)
                .value()
        },

        questionnaire() {
            if (this.selectedQuestionnaire == null) return null

            return find(this.questionnaires, q => {
                const key = q.Id
                return key == this.selectedQuestionnaire.key
            })
        },

        question() {
            if (this.selectedQuestion == null) return null
            return find(this.questions, { Id: this.selectedQuestion.key })
        },

        condition() {
            if (this.selectedCondition == null) return null
            return find(this.questions, { Id: this.selectedCondition.key })
        },

        conditionAnswers() {
            if (this.condition == null) return []
            return filter(this.condition.Answers, ans => this.isSelectedAnswer(ans.Answer))
        },

        // drop down
        selectedQuestion() {
            if (this.query.questionId == null) return null
            return find(this.questionsList, { name: this.query.questionId })
        },

        // drop down
        selectedCondition() {
            if (this.query.conditionId == null) return null
            return find(this.questionsList, { name: this.query.conditionId })
        },

        selectedQuestionnairePlaceholder() {
            return this.loading.questionnaire ? this.$t('Common.Loading') : this.$t('Common.NothingSelected')
        },

        selectedQuestionPlaceholder() {
            return this.loading.questions ? this.$t('Common.Loading') : this.$t('Common.NothingSelected')
        },
    },
}
</script>

<template>
  <div>
    <FilterBlock :title="$t('Reports.Questionnaire')">
      <Typeahead control-id="questionnaire"
        :placeholder="selectedQuestionnairePlaceholder"
        fuzzy
        noClear
        :values="questionnaireList"
        :value="selectedQuestionnaire"
        :forceLoadingState="loading.questionnaire"
        @selected="selectQuestionnaire"
      />
    </FilterBlock>

    <FilterBlock :title="$t('Common.QuestionnaireVersion')">
      <Typeahead control-id="version"
        :placeholder="$t('Common.AllVersions')"
        noSearch
        :values="questionnaireVersionsList"
        :value="selectedQuestionnaireVersion"
        :forceLoadingState="loading.questionnaire"
        :disabled="selectedQuestionnaire == null"
        @selected="selectQuestionnaireVersion"
      />
    </FilterBlock>
    <FilterBlock :title="$t('Reports.Question')">
      <Typeahead control-id="question"
        :placeholder="selectedQuestionPlaceholder"
        fuzzy
        noClear
        :forceLoadingState="loading.questions"
        :values="questionsList"
        :value="selectedQuestion"
        @selected="selectQuestion"
      />
    </FilterBlock>

    <FilterBlock :title="$t('Reports.ViewOptions')" v-if="!isSupervisor && this.question != null">
      <div class="options-group">
        <Radio
          :label="$t('Reports.TeamLeadsOnly')"
          :radioGroup="false"
          name="expandTeams"
          :value="expandTeams"
          @input="radioChanged"
        />
        <Radio
          :label="$t('Reports.WithInterviewers')"
          :radioGroup="true"
          name="expandTeams"
          :value="expandTeams"
          @input="radioChanged"
        />
      </div>
    </FilterBlock>

    <FilterBlock :title="$t('Reports.ByAnswerValue')" v-if="question && question.Type == 'Numeric'">
      <div class="row">
        <div class="col-xs-6">
          <div
            class="form-group"
            v-bind:class="{'has-error': errors.has('min')}"
            :title="errors.first('min')"
          >
            <label for="min">{{ $t("Reports.Min") }}</label>
            <input
              type="number"
              class="form-control input-sm"
              name="min"
              :placeholder="$t('Reports.Min')"
              @input="inputChange"
              v-validate.initial="{ max_value: max }"
              :value="min"
            >
          </div>
        </div>
        <div class="col-xs-6">
          <div
            class="form-group"
            v-bind:class="{'has-error': errors.has('max')}"
            :title="errors.first('max')"
          >
            <label for="max">{{ $t("Reports.Max") }}</label>
            <input
              type="number"
              class="form-control input-sm"
              :placeholder="$t('Reports.Max')"
              v-validate.initial="{ min_value: min }"
              name="max"
              @input="inputChange"
              :value="max"
            >
          </div>
        </div>
      </div>
    </FilterBlock>

    <template v-if="question != null && question.SupportConditions">
      <FilterBlock :title="$t('Reports.ConditionQuestion')">
        <Typeahead control-id="condition"
          :placeholder="$t('Reports.SelectConditionQuestion')"
          :values="conditionVariablesList"
          :value="selectedCondition"
          fuzzy
          @selected="selectCondition"
        />
      </FilterBlock>
      <template v-if="condition != null">
        <Checkbox
          :label="$t('Reports.PivotView')"
          name="pivot"
          :value="query.pivot"
          @input="checkedChange"
        />

        <ul class="list-group small" v-if="!query.pivot">
          <li
            class="list-group-item pointer"
            v-for="answer in condition.Answers"
            :key="answer.Answer"
            :class="{ 'list-group-item-success': isSelectedAnswer(answer.Answer)}"
            @click="selectConditionAnswer(answer.Answer)"
          >{{answer.Answer}}. {{answer.Text}}</li>
        </ul>
      </template>
    </template>
  </div>
</template>

<script>
import Vue from "vue";
import routeSync from "~/shared/routeSync";

export default {
    mixins: [routeSync],

    data() {
        return {
            questionnaires: [],
            questions: [],
            selectedAnswers: [],
            changesQueue: [],
            loading: {
                questions: false,
                questionnaire: false
            }
        };
    },

    props: {
        isSupervisor: false
    },

    watch: {
        filter(filter) {
            this.$emit("input", filter);
        },

        "query.name"(to) {
            if (this.selectedQuestion == null) {
                this.loadQuestions(to);
            }
        },

        selectedQuestion(to) {
            if(to == null && this.questionsList != null && this.questionsList.length > 0) {
                this.selectQuestion(this.questionsList[0]);
            }
        }
    },

    async mounted() {
        await this.loadQuestionnaires();

        if (this.selectedQuestionnaire == null && this.questionnaireList.length > 0) {
            const questionnaire = this.questionnaireList[this.questionnaireList.length - 1];

            this.selectQuestionnaire(questionnaire);
            await this.loadQuestions(questionnaire.key);
        } else {
            await this.loadQuestions();
        }

        if (this.question == null && this.questionsList.length > 0) {
            this.selectQuestion(this.questionsList[0]);
        }

        this.$emit("mounted");
    },

    methods: {
        async loadQuestionnaires() {
            this.loading.questionnaire = true;

            try {
                this.questionnaires = await this.$hq.Report.SurveyStatistics.Questionnaires();
            } finally {
                this.loading.questionnaire = false;
            }
        },

        loadQuestions(questionnaireId = null, version = null) {
            if (questionnaireId == null && this.selectedQuestionnaire == null) return;
            const id = questionnaireId || this.selectedQuestionnaire.key;

            this.loading.questions = true;

            return this.$hq.Report.SurveyStatistics.Questions(id, version)
                .then(questions => {
                    this.questions = questions; 
                    this.loading.questions = false;
                })
                .catch(() => this.loading.questions = false)
        },

        async selectQuestionnaire(id) {
            const questionnaireId = id == null ? null : id.key;

            if (id == null) {
                this.selectQuestion(null);
                this.selectCondition(null);
                return;
            }

            this.selectQuestionnaireVersion(null);

            await this.loadQuestions(id.key, null);
            
            this.selectCondition(null);
            this.onChange(q => (q.name = id.value));

            const question = _.find(this.questionsList, "key", this.query.questionId);
            this.selectQuestion(question);
        },

        selectQuestionnaireVersion(id) {
            const version = id == null ? null : id.key;
            if(this.selectedQuestionnaire == null) return;

            this.loadQuestions(this.selectedQuestionnaire.key, version).then(() => {
                this.onChange(query => (query.version = version));
            });
        },

        selectQuestion(id) {
            this.onChange(query => {
                query.questionId = id == null ? null : id.name;

                if (id != null && !id.SupportConditions) {
                    query.conditionId = null;
                } else {
                    query.conditionId = null;
                    query.expandTeams = false;
                }
            });
        },

        selectCondition(id) {
            this.onChange(query => {
                query.conditionId = id == null ? null : id.name;

                if (id == null) {
                    query.pivot = false;
                }
            });
        },

        isSelectedAnswer(conditionAnswerKey) {
            return _.find(this.selectedAnswers, a => a == conditionAnswerKey) != null;
        },

        selectConditionAnswer(answer) {
            this.selectedAnswers = _.xor(this.selectedAnswers, [answer]);
        }
    },

    computed: {
        filter() {
            const state = this.queryString;

            const filter = _.assign(
                {
                    questionnaireId: this.questionnaire == null ? null : this.questionnaire.Id,
                    questionnaire: this.questionnaire,
                    version: this.version,
                    question: this.question,
                    condition: this.condition,
                    conditionAnswers: this.conditionAnswers
                },
                state
            );

            return filter;
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
                version: this.query.version == "*" ? null : this.query.version
            };
        },

        max() {
            const result = parseInt(this.query.max);
            return _.isNaN(result) ? null : result;
        },

        expandTeams() {
            return this.query.expandTeams === true || this.query.expandTeams === "true";
        },

        min() {
            const result = parseInt(this.query.min);
            return _.isNaN(result) ? null : result;
        },

        questionnaireList() {
            return _.chain(this.questionnaires)
                .orderBy(["Title"], ["asc"])
                .map(q => {
                    return {
                        key: q.Id,
                        value: q.Title
                    };
                })
                .uniqWith(_.isEqual)
                .value();
        },

        questionnaireVersionsList() {
            var val = _.chain(this.questionnaires)
                .filter(c => this.selectedQuestionnaire != null && this.selectedQuestionnaire.key == c.Id)
                .orderBy(["Title", "Version"], ["asc", "asc"])
                .map(q => {
                    return {
                        key: q.Version,
                        value: `ver. ${q.Version}`
                    };
                })
                .uniqWith(_.isEqual)
                .value();

            return val;
        },

        questionsList() {
            function getValue(question) {
                let result = `[${question.VariableName}]`;

                if (question.Label) {
                    result += " " + question.Label + "\r\n" + question.QuestionText;
                } else {
                    result += " " + question.QuestionText;
                }

                return result;
            }

            return _.chain(this.questions)
                .map(q => {
                    return {
                        key: q.Id,
                        name: q.VariableName,
                        supportConditions: q.SupportConditions,
                        value: getValue(q),
                        breadcrumbs: q.Breadcrumbs
                    };
                })
                .value();
        },

        conditionVariablesList() {
            return _.chain(this.questionsList)
                .filter("supportConditions", true)
                .filter(q => q.key != this.selectedQuestion.key)
                .value();
        },

        questionnaire() {
            if (this.selectedQuestionnaire == null) return null;

            return _.find(this.questionnaires, q => {
                const key = q.Id;
                return key == this.selectedQuestionnaire.key;
            });
        },

        question() {
            if (this.selectedQuestion == null) return null;
            return _.find(this.questions, { Id: this.selectedQuestion.key });
        },

        condition() {
            if (this.selectedCondition == null) return null;
            return _.find(this.questions, { Id: this.selectedCondition.key });
        },

        conditionAnswers() {
            if (this.condition == null) return [];
            return _.filter(this.condition.Answers, ans => this.isSelectedAnswer(ans.Answer));
        },

        // drop down
        selectedQuestionnaire() {
            if (this.query.name == null) return null;
            return _.find(this.questionnaireList, { value: this.query.name });
        },

        selectedQuestionnaireVersion() {
            let key = parseInt(this.query.version);
            key = key == NaN ? null : key;
            return _.find(this.questionnaireVersionsList, { key });
        },

        // drop down
        selectedQuestion() {
            if (this.query.questionId == null) return null;
            return _.find(this.questionsList, { name: this.query.questionId });
        },

        // drop down
        selectedCondition() {
            if (this.query.conditionId == null) return null;
            return _.find(this.questionsList, { name: this.query.conditionId });
        },

        selectedQuestionnairePlaceholder() {
            return this.loading.questionnaire ? this.$t("Common.Loading") : this.$t("Common.NothingSelected");
        },

        selectedQuestionPlaceholder() {
            return this.loading.questions ? this.$t("Common.Loading") : this.$t("Common.NothingSelected");
        }
    }
};
</script>

<template>
    <div>
        <FilterBlock :title="$t('Reports.Questionnaire')">
            <Typeahead :placeholder="$t('Common.Loading')" fuzzy noClear
                :values="questionnaireList"
                :value="selectedQuestionnaire"                
                :forceLoadingState="loading.questionnaire"
                @selected="selectQuestionnaire" />
        </FilterBlock>        
        <FilterBlock :title="$t('Reports.Question')">
            <Typeahead :placeholder="$t('Reports.SelectQuestion')" fuzzy 
                :forceLoadingState="loading.questions"
                :values="questionsList"
                :value="selectedQuestion"                
                @selected="selectQuestion" />
        </FilterBlock>            

        <FilterBlock :title="$t('Reports.ViewOptions')" v-if="!isSupervisor && this.question != null">
            <div class="options-group" >
                <Radio :label="$t('Reports.TeamLeadsOnly')" 
                    radioGroup="TeamLeads" name="mode" 
                    :value="query.mode" @input="radioChanged" />           
                <Radio :label="$t('Reports.WithInterviewers')" 
                    radioGroup="WithInterviewers" name="mode"                     
                    :value="query.mode" @input="radioChanged" />
            </div>
        </FilterBlock>        
                
        <FilterBlock :title="$t('Reports.ByAnswerValue')" 
                v-if="question && question.Type == 'Numeric'">
            <div class="row">
                <div class="col-xs-6">
                    <div class="form-group">
                        <label for="min">{{ $t("Reports.Min") }}</label>
                        <input type="number" class="form-control input-sm" name="min"
                            :placeholder="$t('Reports.Min')"
                            @input="inputChange" 
                            :value="min">
                    </div>        
                </div>
                <div class="col-xs-6">
                    <div class="form-group">
                        <label for="max">{{ $t("Reports.Max") }}</label>
                        <input type="number" class="form-control input-sm" 
                            :placeholder="$t('Reports.Max')"                                
                            name="max" @input="inputChange" 
                            :value="max">
                    </div>        
                </div>
            </div>
        </FilterBlock>            

        <template v-if="question != null && question.SupportConditions">
            <FilterBlock :title="$t('Reports.ConditionQuestion')">
                <Typeahead :placeholder="$t('Reports.SelectConditionQuestion')" 
                    :values="conditionVariablesList"
                    :value="selectedCondition"
                    fuzzy
                    @selected="selectCondition" />
            </FilterBlock>
            <template v-if="condition != null">
                  <Checkbox :label="$t('Reports.PivotView')" name="pivot"
                    :value="query.pivot" @input="checkedChange" />

                <ul class="list-group small" v-if="!query.pivot">                
                    <li class="list-group-item pointer"
                        v-for="answer in condition.Answers" :key="answer.Answer"                    
                        :class="{ 'list-group-item-success': isSelectedAnswer(answer.Answer)}"
                        @click="selectConditionAnswer(answer.Answer)" >
                        {{answer.Answer}}. {{answer.Text}}
                    </li>                    
                </ul>
            </template>
        </template>
    </div>
</template>

<script>

function formatGuid(val) {
    return val.split("-").join("")
}

const ReportMode = {
    TeamLeads: "TeamLeads",
    WithInterviewers: "WithInterviewers"
}

export default {
   data() {
     return {
           questionnaires: [],
           questions: [],
           selectedAnswers: [],
           loading: {  
               questions: false,
               questionnaire: false
           }
     };
  },
  
   props: {
     isSupervisor: false
   },

  async mounted() {      
    await this.loadQuestionnaires();

    if(this.selectedQuestionnaire == null && this.questionnaireList.length > 0) {
        this.selectQuestionnaire(this.questionnaireList[this.questionnaireList.length - 1])
    }

    await this.loadQuestions();

    if(this.question == null && this.questionsList.length > 0) {
        this.selectQuestion(this.questionsList[0])
    }

    this.onChange()
  },

  methods: {
    async loadQuestionnaires() {
        this.loading.questionnaire = true
        
        try {
            this.questionnaires = await this.$hq
              .Report.SurveyStatistics
              .Questionnaires()
        } finally {
            this.loading.questionnaire = false
        }
    },

    async loadQuestions() {
        if(this.query.questionnaireId == null)
            return
        
        this.loading.questions = true
        try {   
            this.questions = await this.$hq
                .Report.SurveyStatistics
                .Questions(this.query.questionnaireId); 
        } finally {
            this.loading.questions = false
        }
    },

    selectQuestionnaire(id) {
        const questionnaireId = id == null ? null : id.key

        if(id == null) {
            this.selectQuestion(null)
        }
        this.selectCondition(null)
        this.onChange({ questionnaireId })

        this.loadQuestions()
    },

    selectQuestion(id) {
        this.onChange(query => {
            query.questionId = id == null ? null : id.name

            if(id != null && !id.SupportConditions){
                query.conditionId = null
            } else {
                query.conditionId = null
                query.mode = this.getDefaultMode()
            }
        })
    },

    getDefaultMode() {
        return ReportMode.TeamLeads 
    },

    selectCondition(id) {
        this.onChange(query => {
            query.conditionId = id == null ? null : id.name
            
            if(id == null){
                query.pivot = false
            }
        })
    },

    checkedChange(ev) {
        this.onChange({
            [ev.name]: ev.checked
        })
    },

    radioChanged(ev) {
        this.onChange({
            [ev.name]: ev.selected
        })
    },

    inputChange(ev) {
        const source = ev.target
        let value = source.value;

        if(source.type == 'number') {
            const intValue = parseInt(value)
            value = _.isNaN(intValue) ? null : intValue
        }

        return this.onChange({
            [source.name]: value
        })
    },

    onChange(options = null) {
        let data = {}

        if(typeof(options) == 'function') {
            const result = options(data)
            if(result != null){
                data = result
            }
        } else {
            data = options
        }

        const state = Object.assign(_.clone(this.queryString), data)       

        if(state.min != null 
            && state.max != null 
            && state.min > state.max) return false

        if(data != null) {
            this.updateRoute(state)
        }

        this.$emit("input", Object.assign({
            questionnaire: this.questionnaire,
            question: this.question,
            condition: this.condition,
            conditionAnswers: this.conditionAnswers
        }, state))
    },

    updateRoute(newQuery) {
        const query = {}

        // clean up uri from default values
        Object.keys(newQuery).forEach(key => {
            if(newQuery[key] && !_.isNaN(newQuery[key])) {
                query[key] = newQuery[key]
            }
        })

        this.$router.push({ query });
    },

    isSelectedAnswer(conditionAnswerKey){
        return _.find(this.selectedAnswers, a => a == conditionAnswerKey) != null
    },

    selectConditionAnswer(answer) {
        this.selectedAnswers = _.xor(this.selectedAnswers, [ answer ])

        this.onChange({})
    }
  },

  computed: {      
    query() {
        return this.$store.state.route.query
    },

    queryString() {
        return {
            questionnaireId: this.query.questionnaireId,
            questionId: this.query.questionId,
            conditionId: this.query.conditionId,
            ans: this.condition != null ? this.selectedAnswers : null,
            mode: this.query.mode || this.getDefaultMode(),
            pivot: this.query.pivot,
            min: this.min,
            max: this.max
        }
    },    

    max() {
        const result = parseInt(this.$route.query.max)
        return _.isNaN(result) ? null : result;
    },
    
    min() {
        const result =  parseInt(this.$route.query.min)
        return _.isNaN(result) ? null : result;
    },
    
    questionnaireList() {
        return _.chain(this.questionnaires)
            .orderBy(['Title', 'Version'],['asc', 'asc'])
            .map(q => {
                return {
                    key: formatGuid(q.QuestionnaireId) + "$" + q.Version,
                    value: `(ver. ${q.Version}) ${q.Title}`
                };
        }).value();
    },

    questionsList() {
        function getValue(question) {
            let result = `[${question.StataExportCaption}]`

            if(question.Label) {
                result += ' ' + question.Label + '\r\n' + question.QuestionText
            } else {
                result += ' ' + question.QuestionText
            }

            result += question.QuestionText
            return result
        }

        return _.chain(this.questions).map(q => {
            return {
                key: q.PublicKey,
                pivotable: q.Pivotable,
                name: q.StataExportCaption,
                supportConditions: q.SupportConditions,
                value: getValue(q),
                breadcrumbs: q.Breadcrumbs
            };
      }).value();
    },
    
    conditionVariablesList() {
        return _.chain(this.questionsList)
            .filter('supportConditions', true)
            .filter(q => q.key != this.selectedQuestion.key)
            .value();
    },

    questionnaire() {
        if(this.selectedQuestionnaire != null) {

            const questionnaire = _.find(this.questionnaires, q => 
            { 
                const key = formatGuid(q.QuestionnaireId) + "$" + q.Version               
                return key == this.selectedQuestionnaire.key 
            })
            return questionnaire
        }

        return null
    },

    question() {
        if(this.selectedQuestion != null) {
            return _.find(this.questions, 
            { PublicKey : this.selectedQuestion.key })
        }

        return null
    },

    condition() {
        if(this.selectedCondition == null) return null

        return _.find(this.questions, { PublicKey: this.selectedCondition.key })
    },

    conditionAnswers() {
        if(this.condition == null) return []

        return _.filter(this.condition.Answers, 
            ans => this.isSelectedAnswer(ans.Answer))            
    },

    // drop down
    selectedQuestionnaire() {
        if(this.query.questionnaireId != null) {
            return _.find(this.questionnaireList, { key : this.query.questionnaireId })
        }

        return null
    },

    // drop down
    selectedQuestion() {
        if(this.query.questionId == null) return null
        return _.find(this.questionsList, { name: this.query.questionId })
    },

    // drop down
    selectedCondition() {
        if(this.query.conditionId == null) return null
        return _.find(this.questionsList, { name: this.query.conditionId })
    }
  }
};
</script>

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
            <Typeahead :placeholder="$t('Common.Loading')" fuzzy noClear
                :forceLoadingState="loading.questions"
                :values="questionsList"
                :value="selectedQuestion"                
                @selected="selectQuestion" />
        </FilterBlock>            

        <FilterBlock :title="$t('Reports.ViewOptions')" v-if="!isSupervisor && this.question != null">
            <div class="options-group" >
                <Radio :label="$t('Reports.TeamLeadsOnly')" 
                    :radioGroup="false" name="expandTeams" 
                    :value="expandTeams" @input="radioChanged" />           
                <Radio :label="$t('Reports.WithInterviewers')" 
                    :radioGroup="true" name="expandTeams"                     
                    :value="expandTeams" @input="radioChanged" /> 
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

   watch: {
       filter(filter) {
           this.$emit("input", filter)
       }
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

    this.$emit("mounted")
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

    async selectQuestionnaire(id) {
        const questionnaireId = id == null ? null : id.key

        if(id == null) {
            this.selectQuestion(null)
        }
        
        this.selectCondition(null)
        this.onChange(q => q.questionnaireId = questionnaireId )

        await this.loadQuestions()
        
        const question = _.find(this.questionsList, 'key', this.query.questionId)
        this.selectQuestion(question)
    },

    selectQuestion(id) {
        this.onChange(query => {
            query.questionId = id == null ? null : id.name

            if(id != null && !id.SupportConditions){
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
            
            if(id == null){
                query.pivot = false
            }
        })
    },

    checkedChange(ev) {
        this.onChange(q => {
            q[ev.name] = ev.checked
        })
    },

    radioChanged(ev) {
        this.onChange(q => {
            q[ev.name] = ev.selected
        })
    },

    inputChange(ev) {
        const source = ev.target
        let value = source.value;

        if(source.type == 'number') {
            const intValue = parseInt(value)
            value = _.isNaN(intValue) ? null : intValue
        }

        return this.onChange(q => {
            q[source.name] = value
        })
    },

    onChange(change) {
        let data = {}

        change(data)       

        const state = Object.assign(_.clone(this.queryString), data)       

        if(state.min != null 
            && state.max != null 
            && state.min > state.max) return false

        if(data != null) {
            this.updateRoute(state)
        }
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
    }
  },

  computed: {
    filter() {
        const state = this.queryString

        const filter = Object.assign({
            questionnaire: this.questionnaire,
            question: this.question,
            condition: this.condition,
            conditionAnswers: this.conditionAnswers
        }, state)

        return filter
    },   

    query() {
        return this.$store.state.route.query
    },

    queryString() {
        return {
            questionnaireId: this.query.questionnaireId,
            questionId: this.query.questionId,
            conditionId: this.query.conditionId,
            ans: this.condition != null ? this.selectedAnswers : null,
            expandTeams: this.expandTeams,
            pivot: this.query.pivot,
            min: this.min,
            max: this.max
        }
    },    

    max() {
        const result = parseInt(this.query.max)
        return _.isNaN(result) ? null : result;
    },

    expandTeams() {
        return this.query.expandTeams === true || this.query.expandTeams === 'true'
    },
    
    min() {
        const result =  parseInt(this.query.min)
        return _.isNaN(result) ? null : result;
    },
    
    questionnaireList() {
        return _.chain(this.questionnaires)
            .orderBy(['Title', 'Version'],['asc', 'asc'])
            .map(q => {
                return {
                    key: q.Identity,
                    value: `(ver. ${q.Version}) ${q.Title}`
                };
        }).value();
    },

    questionsList() {
        function getValue(question) {
            let result = `[${question.VariableName}]`

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
                key: q.Id,
                name: q.VariableName,
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
        if(this.selectedQuestionnaire == null) return null
        
        return _.find(this.questionnaires, q => 
        { 
            const key = q.Identity
            return key == this.selectedQuestionnaire.key 
        })
    },

    question() {
        if(this.selectedQuestion == null) return null
        return _.find(this.questions, { Id : this.selectedQuestion.key })        
    },

    condition() {
        if(this.selectedCondition == null) return null
        return _.find(this.questions, { Id: this.selectedCondition.key })
    },

    conditionAnswers() {
        if(this.condition == null) return []
        return _.filter(this.condition.Answers, ans => this.isSelectedAnswer(ans.Answer))            
    },

    // drop down
    selectedQuestionnaire() {
        if(this.query.questionnaireId == null) return null        
        return _.find(this.questionnaireList, { key : this.query.questionnaireId })
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

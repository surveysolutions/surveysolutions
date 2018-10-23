export function shouldShowAnsweredOptionsOnlyForMulti(question){
    var isSupervisorOnsupervisorQuestion = question.$me.isForSupervisor && !question.$me.isDisabled;
    return !isSupervisorOnsupervisorQuestion && !question.showAllOptions && question.$store.getters.isReviewMode && !question.noOptions && question.$me.answer.length > 0 && 
           question.$me.answer.length < question.$me.options.length;
}

export function shouldShowAnsweredOptionsOnlyForSingle(question){
    var isSupervisorOnsupervisorQuestion = question.$me.isForSupervisor && !question.$me.isDisabled;
    return !isSupervisorOnsupervisorQuestion && !question.showAllOptions && question.$store.getters.isReviewMode && !question.noOptions && question.$me.answer;
}



export function shouldShowAnsweredOptionsOnlyForMulti(question) {
    var isSupervisorOnsupervisorQuestion = question.$me.isForSupervisor && !question.$me.isDisabled
    return !isSupervisorOnsupervisorQuestion 
        && !question.showAllOptions && question.$store.getters.isReviewMode 
        && !question.noOptions && question.$me.answer.length > 0 
        && question.$me.answer.length < question.$me.options.length
}

export function shouldShowAnsweredOptionsOnlyForSingle(question) {
    var isSupervisorOnsupervisorQuestion = question.$me.isForSupervisor && !question.$me.isDisabled
    return !isSupervisorOnsupervisorQuestion && !question.showAllOptions && question.$store.getters.isReviewMode && !question.noOptions && question.$me.answer
}


export function getGroupSeparator(question) {
    const defaultSeparator = ''

    if (question.useFormatting) {
        const etalon = 1111
        const localizedNumber = etalon.toLocaleString()
        const separator = localizedNumber.substring(1, localizedNumber.length - 3)
        return (separator == null || separator == undefined)
            ? defaultSeparator
            : separator
    }

    return defaultSeparator
}

export function getDecimalSeparator() {
    const defaultSeparator = '.'
    const etalon = 1.111
    const localizedNumber = etalon.toLocaleString()
    const separator = localizedNumber.substring(1, localizedNumber.length - 3)
    return (separator == null || separator == undefined)
        ? defaultSeparator
        : separator
}

export function getDecimalPlacesCount(question) {
    if (question.countOfDecimalPlaces == null || question.countOfDecimalPlaces == undefined)
        return 15

    return question.countOfDecimalPlaces
}

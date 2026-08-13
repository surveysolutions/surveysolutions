const highlightStartDelayInMs = 350
const highlightDurationInMs = 1300
const jumpTargetHighlightClass = 'jump-target-highlight'

function getQuestionContainer(targetElement) {
    if (!targetElement) return null
    if (targetElement.classList?.contains('question')) return targetElement
    return targetElement.closest?.('.question') ?? null
}

export function flashJumpTarget(targetElement) {
    const questionElement = getQuestionContainer(targetElement)
    if (!questionElement) return

    if (questionElement.jumpTargetHighlightStartTimeoutId)
        clearTimeout(questionElement.jumpTargetHighlightStartTimeoutId)
    if (questionElement.jumpTargetHighlightEndTimeoutId)
        clearTimeout(questionElement.jumpTargetHighlightEndTimeoutId)

    questionElement.jumpTargetHighlightStartTimeoutId = setTimeout(() => {
        questionElement.classList.remove(jumpTargetHighlightClass)
        void questionElement.offsetWidth
        questionElement.classList.add(jumpTargetHighlightClass)
        questionElement.jumpTargetHighlightEndTimeoutId = setTimeout(() => {
            questionElement.classList.remove(jumpTargetHighlightClass)
        }, highlightDurationInMs)
    }, highlightStartDelayInMs)
}

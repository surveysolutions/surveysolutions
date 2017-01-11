declare interface IQuestionnaireInfo {
    title: string;
}

declare interface IWebInterviewApi {
    questionnaireDetails(questionnaireId: string): IQuestionnaireInfo
    createInterview(questionnaireId: string): string
    startInterview(entity: string): void
    getPrefilledQuestions(): any

    getEntityDetails(id: string): any

    answerSingleOptionQuestion(answer: number, questionId: string)
    answerTextQuestion(questionIdentity: string, text: string): void

    removeAnswer(questionId: string): void
}

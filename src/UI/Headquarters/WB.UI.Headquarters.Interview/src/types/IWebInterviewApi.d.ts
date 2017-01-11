declare interface IQuestionnaireInfo {
    title: string;
}

declare interface IInterviewEntityWithType {
    entityType: string,
    identity: string
}

declare interface IPrefilledPageData {
    firstSectionId: string,
    questions: IInterviewEntityWithType[]
}

declare interface IWebInterviewApi {
    questionnaireDetails(questionnaireId: string): IQuestionnaireInfo
    createInterview(questionnaireId: string): string
    startInterview(entity: string): void
    getPrefilledPageData(): IPrefilledPageData

    getEntityDetails(id: string): any

    answerSingleOptionQuestion(answer: number, questionId: string)
    answerTextQuestion(questionIdentity: string, text: string): void

    removeAnswer(questionId: string): void
}

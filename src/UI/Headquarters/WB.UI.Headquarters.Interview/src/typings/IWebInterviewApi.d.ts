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

declare interface ISectionData {
    id:string
    type:string
    status: string
    entities: any[]
}

declare interface IInterviewDetails {
    sections: ISectionData[]
}

declare interface IWebInterviewApi {
    questionnaireDetails(questionnaireId: string): IQuestionnaireInfo
    createInterview(questionnaireId: string): string

    getPrefilledQuestions(): ISectionData
    getInterviewSections(): IInterviewDetails
    getSectionDetails(sectionId: string): ISectionData

    getEntityDetails(id: string): any

    answerSingleOptionQuestion(answer: number, questionId: string)
    answerMutliOptionQuestion(answer: number, questionId: string)
    answerTextQuestion(questionIdentity: string, text: string): void
    answerIntegerQuestion(questionIdentity: string, answer: number): void
    answerDoubleQuestion(questionIdentity: string, answer: number): void

    removeAnswer(questionId: string): void
}

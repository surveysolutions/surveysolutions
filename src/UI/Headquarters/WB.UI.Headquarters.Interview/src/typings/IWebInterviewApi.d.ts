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

declare interface IInterviewDetails {
    sections: ISectionInfo[]
}

declare interface ISectionData {
    entities: IInterviewEntity[]
    info: ISectionInfo
    breadcrumps: ISectionBreadcrumps
}

declare interface ISectionInfo {
    id: string
    type: string
    status: string
}

declare interface IInterviewEntity {
    id: string
    title: string
    isDisabled: boolean
    hideIfDisabled: boolean
}

declare interface ISectionBreadcrumps {
    title: string
}

declare interface IWebInterviewApi {
    questionnaireDetails(questionnaireId: string): IQuestionnaireInfo
    createInterview(questionnaireId: string): string

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

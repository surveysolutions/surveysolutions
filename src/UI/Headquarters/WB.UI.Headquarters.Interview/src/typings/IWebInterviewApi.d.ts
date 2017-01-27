declare interface IQuestionnaireInfo {
    title: string;
}

declare interface IInterviewEntityWithType {
    entityType: string,
    identity: string
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

declare interface IBreadcrumpInfo {
    status: string
    breadcrumbs: ISectionBreadcrumps[]
}

declare enum GroupStatus {
    Completed = 1,
    Invalid = -1,
    Other = 0,
}

declare interface ISidebarPanel {
    id: string
    title: string
    state: GroupStatus
    panels: ISidebarPanel[]
}

declare interface IWebInterviewApi {
    questionnaireDetails(questionnaireId: string): IQuestionnaireInfo

    getPrefilledEntities(): IInterviewEntityWithType[]
    getSectionEntities(sectionId: string): IInterviewEntityWithType[]
    getEntityDetails(id: string): IInterviewEntity
    getEntitiesDetails(ids: string[]): IInterviewEntity[]
    getBreadcrumbs(): IBreadcrumpInfo
    getSidebarState(): ISidebarPanel[]

    answerSingleOptionQuestion(answer: number, questionId: string)
    answerMultiOptionQuestion(answer: number, questionId: string)
    answerTextQuestion(questionIdentity: string, text: string): void
    answerIntegerQuestion(questionIdentity: string, answer: number): void
    answerDoubleQuestion(questionIdentity: string, answer: number): void
    answerDateQuestion(questionIdentity: string, answer: Date): void

    removeAnswer(questionId: string): void
}

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

declare interface IValidity {
    isValid: Boolean

}

declare interface ISidebarPanel {
    id: string
    parentId: string
    title: string
    state: GroupStatus
    collapsed: Boolean
    hasChildren: Boolean
    validity: IValidity
}
declare interface ITextListAnswerRow {
    value: number
    text: string
}

declare interface IDropdownItem {
    value: number
    title: string
}

declare interface IWebInterviewApi {
    questionnaireDetails(questionnaireId: string): IQuestionnaireInfo

    getPrefilledEntities(): IInterviewEntityWithType[]
    getSectionEntities(sectionId: string): IInterviewEntityWithType[]
    getEntityDetails(id: string): IInterviewEntity
    getEntitiesDetails(ids: string[]): IInterviewEntity[]
    getBreadcrumbs(): IBreadcrumpInfo
    getSidebarState(): ISidebarPanel[]
    getTopFilteredOptionsForQuestion(id: string, filter: string, count: number): IDropdownItem[]
    getSidebarChildSectionsOf(ids: string[]): ISidebarPanel[]

    answerSingleOptionQuestion(answer: number, questionId: string)
    answerMultiOptionQuestion(answer: number, questionId: string)
    answerYesNoQuestion(questionId: string, answer: string)
    answerTextQuestion(questionIdentity: string, text: string): void
    answerIntegerQuestion(questionIdentity: string, answer: number): void
    answerDoubleQuestion(questionIdentity: string, answer: number): void
    answerDateQuestion(questionIdentity: string, answer: Date): void

    answerTextListQuestion(questionIdentity: string, rows: ITextListAnswerRow[]): void

    removeAnswer(questionId: string): void
}

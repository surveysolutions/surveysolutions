declare interface ILanguageInfo {
    currentLanguage: string
    languages: string[]
}

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

declare interface IGpsAnswer {
    latitude: number
    longitude: number
    accuracy: number
    altitude: number
    timestamp: number
}

declare interface IDropdownItem {
    value: number
    title: string
}

declare interface IPrefilledPageData {
    entities: IInterviewEntityWithType[]
    firstSectionId: string
    hasAnyQuestions: boolean
}

declare interface IWebInterviewApi {
    questionnaireDetails(questionnaireId: string): IQuestionnaireInfo

    getPrefilledEntities(): IPrefilledPageData
    getSectionEntities(sectionId: string): IInterviewEntityWithType[]
    getEntityDetails(id: string): IInterviewEntity
    getEntitiesDetails(ids: string[]): IInterviewEntity[]
    getBreadcrumbs(): IBreadcrumpInfo
    getSidebarState(): ISidebarPanel[]
    getTopFilteredOptionsForQuestion(id: string, filter: string, count: number): IDropdownItem[]
    getSidebarChildSectionsOf(ids: string[]): ISidebarPanel[]

    answerSingleOptionQuestion(answer: number, questionId: string): void
    answerMultiOptionQuestion(answer: number, questionId: string): void
    answerYesNoQuestion(questionId: string, answer: Object[]): void
    answerTextQuestion(questionIdentity: string, text: string): void
    answerIntegerQuestion(questionIdentity: string, answer: number): void
    answerDoubleQuestion(questionIdentity: string, answer: number): void
    answerDateQuestion(questionIdentity: string, answer: Date): void
    answerLinkedSingleOptionQuestion(questionIdentity: string, answer: number[]): void
    answerLinkedToListSingleQuestion(questionIdentity: string, answer: number): void
    answerLinkedMultiOptionQuestion(questionIdentity: string, answer: number[][]): void
    answerLinkedToListMultiQuestion(questionIdentity: string, answer: number[]): void
    answerTextListQuestion(questionIdentity: string, rows: ITextListAnswerRow[]): void
    answerGpsQuestion(identity, answer: IGpsAnswer)

    removeAnswer(questionId: string): void
    getLanguageInfo(): ILanguageInfo
    changeLanguage(language: string): void
}

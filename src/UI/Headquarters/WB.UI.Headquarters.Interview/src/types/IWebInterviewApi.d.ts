declare interface IQuestionnaireInfo {
    title:string;
}

declare interface IWebInterviewApi {
    questionnaireDetails(questionnaireId: string):IQuestionnaireInfo
    createInterview(questionnaireId: string):string
    getTextQuestion(entity: InterviewEntity): any
    getSingleOptionQuestion(entity: InterviewEntity): any
    startInterview(entity: string): void
    getPrefilledQuestions(): any

    answerSingleOptionQuestion(answer:number, questionId:string)
    answerTextQuestion(questionIdentity: string, text: string): void
}

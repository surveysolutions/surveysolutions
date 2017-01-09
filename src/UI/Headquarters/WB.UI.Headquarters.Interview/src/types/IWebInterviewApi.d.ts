declare interface IWebInterviewApi {
    getTextQuestion(entity: InterviewEntity): any
    getSingleOptionQuestion(entity: InterviewEntity): any
    startInterview(entity: string): void
    getPrefilledQuestions(): any
}

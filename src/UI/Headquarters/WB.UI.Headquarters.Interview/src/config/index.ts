declare const INTERVIEW_APP_CONFIG: any

export const virtualPath: string = INTERVIEW_APP_CONFIG.virtualPath
export const imageUploadUri: string = INTERVIEW_APP_CONFIG.imageUploadUri
export const audioUploadUri: string = INTERVIEW_APP_CONFIG.audioUploadUri
export const imageGetBase: string = INTERVIEW_APP_CONFIG.imageGetBase
export const signalrPath: string = INTERVIEW_APP_CONFIG.signalrPath
export const signalrUrlOverride: string = INTERVIEW_APP_CONFIG.signalrUrlOverride
export const supportedTransports: string[] = ["webSockets", "longPolling"]
export const verboseMode = INTERVIEW_APP_CONFIG.verboseLogging
export const assetsPath = INTERVIEW_APP_CONFIG.assetsPath
export const appVersion = INTERVIEW_APP_CONFIG.appVersion
export const googleApiKey: string = INTERVIEW_APP_CONFIG.googleApiKey

declare var INTERVIEW_APP_CONFIG: any

export const virtualPath: string = INTERVIEW_APP_CONFIG.virtualPath
export const imageUri: string = INTERVIEW_APP_CONFIG.imageUploadUri
export const signalrPath: string = INTERVIEW_APP_CONFIG.signalrPath
export const signalrUrlOverride: string = INTERVIEW_APP_CONFIG.signalrUrlOverride
export const supportedTransports: string[] = ["webSockets", "longPolling"]
export const verboseMode = INTERVIEW_APP_CONFIG.verboseLogging

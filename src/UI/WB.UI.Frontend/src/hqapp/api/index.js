import axios from "axios";

class QuestionnaireApi {
    constructor(questionnaireId, version, http) {
        this.http = http
        this.base = "api/v1/questionnaires/"
        this.details = this.base + `${questionnaireId}/${version}`
        this.questionnaireId = questionnaireId
        this.version = version
    }

    async List() {
        const result = []
        let offset = 1
        
        while(offset > 0){
            const response = await this.http.get(this.base, { params: { limit: 40, offset}})
            const data = response.data
            offset = data.Offset + 1
            
            data.Questionnaires.forEach(q => result.push(q))

            if(data.Questionnaires == null || data.Questionnaires.length == 0) break
        }

        return result
    }

    Delete() {
        var self = this
        return this.http.post("api/QuestionnairesApi/DeleteQuestionnaire", 
            { questionnaireId: self.questionnaireId, version: self.version })
    }

    AudioAudit(enabled) {
        const url = `${this.base}${this.questionnaireId}/${this.version}/recordAudio`
        return this.http.post(url, { enabled: enabled })
    }
}

class SurveyStatistics {
    constructor(http) {
        this.reportPath = "api/v1/statistics"
        this.http = http
    }

    async Questions(questionnaireId, version) {
        return (await this.http.get(`${this.reportPath}/questions`, {
            params: { questionnaireId, version }
        })).data
    }

    async Questionnaires() {
        return (await this.http.get(`${this.reportPath}/questionnaires`)).data
    }

    get Uri() {
        return this.http.defaults.baseURL + this.reportPath
    }
}

class MapsReport {
    constructor(http) {
        this.http = http
    }

    async GpsQuestionsByQuestionnaire(questionnaireId, version){ 
        return await this.http.get(`api/ReportDataApi/QuestionInfo/${questionnaireId}?version=${version}`)
    }

    async Report(request) {
        return await this.http.post('api/ReportDataApi/MapReport', request)
    }

    async InteriewSummaryUrl(interviewId) {
        var response = await this.http.post('api/InterviewApi/InterviewSummaryForMapPoint', 
        {
            interviewId
        })

        return response
    }

    GetInterviewDetailsUrl(interviewId) {
        return `${this.http.defaults.baseURL}Interview/Review/${interviewId}`
    }
}

class Users {
    constructor(http){
        this.http = http
    }

    async Supervisors(filter) {
        return (await this.http.get('api/v1/users/supervisors', {
            params: { filter }
        })).data
    }

    get SupervisorsUri() {
        return this.http.defaults.baseURL + 'api/v1/users/supervisors'
    }
}

class Reports {
    constructor(http, basePath){
        this.http = http
        this.basePath = basePath
    }

    get SurveyStatistics() {
        return new SurveyStatistics(this.http)
    }

    get MapReport() {
        return new MapsReport(this.http)
    }

    Chart({questionnaireId, version, from, to}) {
        return this.http.post('api/ReportDataApi/ChartStatistics', {
            templateId: questionnaireId,
            templateVersion: version,
            from,
            to
        })
    }
}

class AssignmentsApi {
    constructor(http) {
        this.http = http
        this.base = "api/v1/assignments"
    }

    async audioSettings(assignmentId) {
        var url = `${this.base}/${assignmentId}/recordAudio`

        const response = await this.http.get(url)
        const responseData = response.data

        return responseData
    }

    setAudioSettings(assignmentId, isEnabled) {
        var url = `${this.base}/${assignmentId}/recordAudio`

        return this.http.patch(url,  {enabled: isEnabled})
    }

    async quantitySettings(assignmentId) {
        var url = `${this.base}/${assignmentId}/assignmentQuantitySettings`

        const response = await this.http.get(url)
        const responseData = response.data

        return responseData
    }
    
    changeQuantity(assignmentId, targetQuantity) {
        var url = `${this.base}/${assignmentId}/changeQuantity`
        return this.http({
            method: 'patch',
            url: url,
            data: targetQuantity,
            headers: {
                accept: 'text/plain',
                'content-type': "application/json"
            }
        })
    }
}

class WebInterviewSettingsApi {
    constructor(http) {
        this.http = http
        this.base = "api/v1/webInterviewSettings"
    }

    /*async fetchEmailTemplates(questionnaireId) {
        var url = `${this.base}/${questionnaireId}/emailTemplates`

        const response = await this.http.get(url)
        const responseData = response.data

        return responseData
    }*/

    updateEmailTemplate(questionnaireId, type, subject, message, passwordDescription, linkText) {
        var url = `${this.base}/${questionnaireId}/emailTemplate`;
        return this.http.post(url, { type: type, subject: subject, message: message, passwordDescription: passwordDescription, linkText:linkText });
    }

    updatePageMessage(questionnaireId, titleType, titleText, messageType, messageText) {
        var url = `${this.base}/${questionnaireId}/pageTemplate`;
        return this.http.post(url, { titleType: titleType, titleText: titleText, messageType: messageType, messageText: messageText });
    }

    /*updateReminderSettings(questionnaireId, reminderAfterDaysIfNoResponse, reminderAfterDaysIfPartialResponse) {
        var url = `${this.base}/${questionnaireId}/reminderSettings`;
        return this.http.post(url, { reminderAfterDaysIfNoResponse: reminderAfterDaysIfNoResponse, reminderAfterDaysIfPartialResponse: reminderAfterDaysIfPartialResponse });
    }

    updateSpamProtection(questionnaireId, isEnabled) {
        var url = `${this.base}/${questionnaireId}/spamProtection`;
        return this.http.post(url, { isEnabled: isEnabled });
    }*/

    updateAdditionalSettings(questionnaireId, 
        isEnabledSpamProtection, 
        reminderAfterDaysIfNoResponse,
         reminderAfterDaysIfPartialResponse,
         singleResponse) {
        var url = `${this.base}/${questionnaireId}/additionalSettings`;
        return this.http.post(url, { 
            spamProtection: isEnabledSpamProtection, 
            reminderAfterDaysIfNoResponse: reminderAfterDaysIfNoResponse,
            reminderAfterDaysIfPartialResponse: reminderAfterDaysIfPartialResponse,
            singleResponse: singleResponse
        });
    }

    startWebInterview(questionnaireId) {
        var url = `${this.base}/${questionnaireId}/start`;
        return this.http.post(url, {});
    }

    stopWebInterview(questionnaireId) {
        var url = `${this.base}/${questionnaireId}/stop`;
        return this.http.post(url, {});
    }
}

class ExportSettings {
    constructor(http) {
        this.http = http
        this.base = "api/ExportSettingsApi"
    }

    setEncryption(val) {
        const url = `${this.base}/ChangeState`
        return this.http.post(url, { enableState: val })
    }

    getEncryption() {
        return this.http.get(`${this.base}/ExportSettings`)
    }

    regenPassword(){
        return this.http.post(`${this.base}/RegeneratePassword`)
    }
}

// var $webInterviewSettingsUrl = '@Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "AdminSettings", action = "WebInterviewSettings" })';


class AdminSettings {
    constructor(http) {
        this.http = http
        this.base = "api/AdminSettings"
    }

    getGlobalNotice() {
        return this.http.get(`${this.base}/GlobalNoticeSettings`)
    }
    setGlobalNotice(val) {
        return this.http.post(`${this.base}/GlobalNoticeSettings`, {GlobalNotice: val})
    }
    getProfileSettings() {
        return this.http.get(`${this.base}/ProfileSettings`)
    }
    setProfileSettings(allowInterviewerUpdateProfile) {
        return this.http.post(`${this.base}/ProfileSettings`, {allowInterviewerUpdateProfile: allowInterviewerUpdateProfile})
    }
    setInterviewerSettings(isInterviewerAutomaticUpdatesEnabled, isDeviceNotificationsEnabled){
        return this.http.post(`${this.base}/InterviewerSettings`, 
            { interviewerAutoUpdatesEnabled: isInterviewerAutomaticUpdatesEnabled, 
              notificationsEnabled: isDeviceNotificationsEnabled
            })
    }
    getInterviewerSettings() {
        return this.http.get(`${this.base}/InterviewerSettings`)
    }
    getWebInterviewSettings() {
        return this.http.get(`${this.base}/WebInterviewSettings`)
    }
    setWebInterviewSettings(allowEmails) {
        return this.http.post(`${this.base}/WebInterviewSettings`, {allowEmails: allowEmails})
    }
}

class HttpUtil {
    getCsrfCookie() {
        var name = "CSRF-TOKEN="
        var decodedCookie = decodeURIComponent(document.cookie)
        var ca = decodedCookie.split(';')
        for(var i = 0; i <ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) == 0) {
                return c.substring(name.length, c.length)
            }
        }
        return ""
    }
}

class HqApiClient {
    constructor(basePath) {
        this.basePath = basePath;

        this.http = axios.create({
            baseURL: basePath
        });
    }

    Questionnaire(questionnaireId, version) {
        return new QuestionnaireApi(questionnaireId, version, this.http)
    }

    get Report() { return new Reports(this.http) }

    get Users() { return new Users(this.http) }

    get Assignments() { return new AssignmentsApi(this.http) }

    get WebInterviewSettings() { return new WebInterviewSettingsApi(this.http) }

    get ExportSettings() {
        return new ExportSettings(this.http)
    }

    get AdminSettings(){
        return new AdminSettings(this.http)
    }

    get Util() {
        return new HttpUtil()
    }
}

/*  the Plugin */
export default {
    install: function(vue) {
        const instance = new HqApiClient(vue.$config.basePath);

        // /*  expose a global API method  */
        Object.defineProperty(vue, "$hq", {
            get() {
                return instance;
            }
        });

        /*  expose a local API method  */
        Object.defineProperty(vue.prototype, "$hq", {
            get() {
                return instance;
            }
        });
    }
};

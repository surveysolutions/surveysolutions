import axios from 'axios'
import { map } from 'lodash'

class QuestionnaireApi {
    constructor(questionnaireId, version, http) {
        this.http = http
        this.base = '/api/v1/questionnaires/'
        this.details = this.base + `${questionnaireId}/${version}`
        this.questionnaireId = questionnaireId
        this.version = version
    }

    async List() {
        const result = []
        let offset = 1

        while (offset > 0) {
            const response = await this.http.get(this.base, {
                params: {
                    limit: 40,
                    offset,
                },
            })
            const data = response.data
            offset = data.Offset + 1

            data.Questionnaires.forEach(q => result.push(q))

            if (data.Questionnaires == null || data.Questionnaires.length == 0) break
        }

        return result
    }

    Delete() {
        var self = this
        return this.http.post('api/QuestionnairesApi/DeleteQuestionnaire', {
            questionnaireId: self.questionnaireId,
            version: self.version,
        })
    }

    AudioAudit(enabled) {
        const url = `${this.base}${this.questionnaireId}/${this.version}/recordAudio`
        return this.http.post(url, {
            enabled: enabled,
        })
    }

    async ExposedVariables(id) {
        const response = await this.http.get(`api/QuestionnairesApi/GetQuestionnaireExposedVariables?id=${id}`,
            {
                params: {
                    limit: 100,
                },
            })
        return response.data
    }

    ChangeVariableExposeStatus(questionnaireIdentity, variables){
        return this.http.post('api/QuestionnairesApi/changeVariableExposeStatus', {
            questionnaireIdentity: questionnaireIdentity,
            variables: variables,
        })
    }

}

class SurveyStatistics {
    constructor(http) {
        this.reportPath = 'api/v1/statistics'
        this.http = http
    }

    async Questions(questionnaireId, version) {
        return (await this.http.get(`${this.reportPath}/questions`, {
            params: {
                questionnaireId,
                version,
            },
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

    async GpsQuestionsByQuestionnaire(questionnaireId, version) {
        return await this.http.get(`api/ReportDataApi/QuestionInfo/${questionnaireId}?version=${version}`)
    }

    async Report(request) {
        return await this.http.post('api/ReportDataApi/MapReport', request)
    }

    async InteriewSummaryUrl(interviewId) {
        var response = await this.http.post('api/InterviewApi/InterviewSummaryForMapPoint', {
            interviewId,
        })

        return response
    }

    GetInterviewDetailsUrl(interviewId) {
        return `${this.http.defaults.baseURL}Interview/Review/${interviewId}`
    }
}

class Workspaces {
    constructor(http) {
        this.http = http
    }

    async List(userId, includeDisabled) {
        const response = await this.http.get('api/v1/workspaces',
            {
                params: {
                    userId,
                    includeDisabled,
                    length: 1000,
                },
            })
        return response.data
    }

    Assign(userIds, workspaces, mode = 'Assign') {
        var assignWorkspaces = map(workspaces, w => {
            return {
                workspace: w,
                supervisorId: null,
            }
        })

        return this.http.post('api/v1/workspaces/assign',
            {
                userIds, workspaces: assignWorkspaces, mode,
            }
        )
    }

    AssignInterviewer(userIds, workspace, supervisor, mode = 'Add') {
        var assignWorkspaces = [{
            workspace: workspace,
            supervisorId: supervisor,
        }]

        return this.http.post('api/v1/workspaces/assign',
            {
                userIds, workspaces: assignWorkspaces, mode,
            }
        )
    }

    async Status(workspace) {
        return await this.http.get('api/v1/workspaces/status/' + workspace)
    }

    async Delete(workspace) {
        return await this.http.delete('api/v1/workspaces/' + workspace)
    }

}

class Users {
    constructor(http) {
        this.http = http
    }

    async Supervisors(filter) {
        return (await this.http.get('api/v1/users/supervisors', {
            params: {
                filter,
            },
        })).data
    }

    get SupervisorsUri() {
        return this.http.defaults.baseURL + 'api/v1/users/supervisors'
    }
}

class MapDashboard {
    constructor(http) {
        this.http = http
    }

    async GetMarkers(request) {
        return await this.http.post('api/MapDashboardApi/Markers', request)
    }

    async InteriewSummaryUrl(interviewId) {
        var response = await this.http.post('api/InterviewApi/InterviewSummaryForMapPoint', {
            interviewId,
        })

        return response
    }

    async AssignmentUrl(assignmentId) {
        var response = await this.http.post('api/AssignmentsApi/AssignmentMapPoint', {
            assignmentId,
        })

        return response
    }

    GetInterviewDetailsUrl(interviewId) {
        return `${this.http.defaults.baseURL}Interview/Review/${interviewId}`
    }

    GetAssignmentDetailsUrl(assignmentId) {
        return `${this.http.defaults.baseURL}Assignments/${assignmentId}`
    }
}

class InterviewsPublicApi {
    constructor(http) {
        this.http = http
        this.base = 'api/v1/interviews'
    }

    async SvApprove(interviewId, comment) {
        var url = `${this.base}/${interviewId}/approve`

        const response = await this.http.patch(url, {
            comment,
        })
        const responseData = response.data

        return responseData
    }

    async SvReject(interviewId, comment) {
        var url = `${this.base}/${interviewId}/reject`

        const response = await this.http.patch(url, {
            comment,
        })
        const responseData = response.data

        return responseData
    }

    async HqApprove(interviewId, comment) {
        var url = `${this.base}/${interviewId}/hqapprove`

        const response = await this.http.patch(url, {
            comment,
        })
        const responseData = response.data

        return responseData
    }

    async HqReject(interviewId, comment) {
        var url = `${this.base}/${interviewId}/hqreject`

        const response = await this.http.patch(url, {
            comment,
        })
        const responseData = response.data

        return responseData
    }

    async HqUnapprove(interviewId, comment) {
        var url = `${this.base}/${interviewId}/hqunapprove`

        const response = await this.http.patch(url, {
            comment,
        })
        const responseData = response.data

        return responseData
    }

    async SvAssign(interviewId, responsibleId) {
        var url = `${this.base}/${interviewId}/assignsupervisor`

        const response = await this.http.patch(url, {
            responsibleId,
        })
        const responseData = response.data

        return responseData
    }

    async Assign(interviewId, responsibleId) {
        var url = `${this.base}/${interviewId}/assign`

        const response = await this.http.patch(url, {
            responsibleId,
        })
        const responseData = response.data

        return responseData
    }
}


class Reports {
    constructor(http, basePath) {
        this.http = http
        this.basePath = basePath
    }

    get SurveyStatistics() {
        return new SurveyStatistics(this.http)
    }

    get MapReport() {
        return new MapsReport(this.http)
    }

    Chart({
        questionnaireId,
        version,
        from,
        to,
    }) {
        return this.http.post('api/ReportDataApi/ChartStatistics', {
            templateId: questionnaireId,
            templateVersion: version,
            from,
            to,
        })
    }
}

class AssignmentsApi {
    constructor(http) {
        this.http = http
        this.base = 'api/v1/assignments'
    }

    async audioSettings(assignmentId) {
        var url = `${this.base}/${assignmentId}/recordAudio`

        const response = await this.http.get(url)
        const responseData = response.data

        return responseData
    }

    setAudioSettings(assignmentId, isEnabled) {
        var url = `${this.base}/${assignmentId}/recordAudio`

        return this.http.patch(url, {
            enabled: isEnabled,
        })
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
                'content-type': 'application/json',
            },
        })
    }

    changeMode(assignmentId, isEnabled) {
        var url = `${this.base}/${assignmentId}/changeMode`

        return this.http.patch(url, {
            enabled: isEnabled,
        })
    }
}

class WebInterviewSettingsApi {
    constructor(http) {
        this.http = http
        this.base = 'api/v1/webInterviewSettings'
    }

    /*async fetchEmailTemplates(questionnaireId) {
        var url = `${this.base}/${questionnaireId}/emailTemplates`

        const response = await this.http.get(url)
        const responseData = response.data

        return responseData
    }*/

    updateEmailTemplate(questionnaireId, type, subject, message, passwordDescription, linkText) {
        var url = `${this.base}/${questionnaireId}/emailTemplate`
        return this.http(
            {
                method: 'post',
                url: url,
                data: {
                    type: type,
                    subject: subject,
                    message: message,
                    passwordDescription: passwordDescription,
                    linkText: linkText},
                headers: {'X-CSRF-TOKEN': new HttpUtil().getCsrfCookie()},
            })
    }

    updatePageMessage(questionnaireId, titleType, titleText, messageType, messageText, buttonType, buttonText) {
        var url = `${this.base}/${questionnaireId}/pageTemplate`
        return this.http(
            {
                method: 'post',
                url: url,
                data: {
                    titleType: titleType,
                    titleText: titleText,
                    messageType: messageType,
                    messageText: messageText,
                    buttonType: buttonType,
                    buttonText: buttonText},
                headers: {'X-CSRF-TOKEN': new HttpUtil().getCsrfCookie()},
            })
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
        singleResponse,
        emailOnComplete,
        attachAnswersInEmail,
        allowSwitchToCawiForInterviewer) {
        var url = `${this.base}/${questionnaireId}/additionalSettings`
        return this.http(
            {
                method: 'post',
                url:url,
                data: {
                    spamProtection: isEnabledSpamProtection,
                    reminderAfterDaysIfNoResponse: reminderAfterDaysIfNoResponse,
                    reminderAfterDaysIfPartialResponse: reminderAfterDaysIfPartialResponse,
                    singleResponse: singleResponse,
                    emailOnComplete: emailOnComplete,
                    attachAnswersInEmail: attachAnswersInEmail,
                    allowSwitchToCawiForInterviewer: allowSwitchToCawiForInterviewer},
                headers: {'X-CSRF-TOKEN': new HttpUtil().getCsrfCookie()},
            })
    }

    startWebInterview(questionnaireId) {
        var url = `${this.base}/${questionnaireId}/start`

        return this.http(
            {
                url:url,
                method: 'post',
                headers: {'X-CSRF-TOKEN': new HttpUtil().getCsrfCookie()},
            })
    }

    stopWebInterview(questionnaireId) {
        var url = `${this.base}/${questionnaireId}/stop`
        return this.http(
            {
                url:url,
                method: 'post',
                headers: {'X-CSRF-TOKEN': new HttpUtil().getCsrfCookie()},
            })
    }
}

class ExportSettings {
    constructor(http) {
        this.http = http
        this.base = 'api/ExportSettingsApi'
    }

    setEncryption(val) {
        const url = `${this.base}/ChangeState`
        return this.http(
            {
                method: 'post',
                url: url,
                data:{ enableState: val },
                headers: {'X-CSRF-TOKEN': new HttpUtil().getCsrfCookie()},
            })
    }

    getEncryption() {
        return this.http.get(`${this.base}/ExportSettings`)
    }

    regenPassword() {
        return this.http(
            {
                method: 'post',
                url: `${this.base}/RegeneratePassword`,
                headers: {'X-CSRF-TOKEN': new HttpUtil().getCsrfCookie()},
            })
    }
}

// var $webInterviewSettingsUrl = '@Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "AdminSettings", action = "WebInterviewSettings" })';

class ControlPanel {
    constructor(http) {
        this.http = http
        this.base = 'api/ControlPanelApi'
    }

    getConfiguration() {
        return this.http.get(`${this.base}/Configuration`)
    }

    getApkInfos() {
        return this.http.get(`${this.base}/AppUpdates`)
    }

    getHealthResult() {
        return this.http.get(`${this.base}/GetHealthResult`)
    }

    getMetricsState() {
        return this.http.get(`${this.base}/GetMetricsState`)
    }
}

class AdminSettings {
    constructor(http) {
        this.http = http
        this.base = 'api/AdminSettings'
    }

    getGlobalNotice() {
        return this.http.get(`${this.base}/GlobalNoticeSettings`)
    }
    setGlobalNotice(val) {
        return this.http.post(`${this.base}/GlobalNoticeSettings`, {
            GlobalNotice: val,
        })
    }
    getProfileSettings() {
        return this.http.get(`${this.base}/ProfileSettings`)
    }
    setProfileSettings(allowInterviewerUpdateProfile) {
        return this.http.post(`${this.base}/ProfileSettings`, {
            allowInterviewerUpdateProfile: allowInterviewerUpdateProfile,
        })
    }
    setInterviewerSettings(isInterviewerAutomaticUpdatesEnabled, isDeviceNotificationsEnabled, isPartialSynchronizationEnabled,
        geographyQuestionAccuracyInMeters, geographyQuestionPeriodInSeconds) {
        return this.http.post(`${this.base}/InterviewerSettings`, {
            interviewerAutoUpdatesEnabled: isInterviewerAutomaticUpdatesEnabled,
            notificationsEnabled: isDeviceNotificationsEnabled,
            partialSynchronizationEnabled: isPartialSynchronizationEnabled,
            geographyQuestionAccuracyInMeters: geographyQuestionAccuracyInMeters,
            geographyQuestionPeriodInSeconds: geographyQuestionPeriodInSeconds,
        })
    }
    getInterviewerSettings() {
        return this.http.get(`${this.base}/InterviewerSettings`)
    }
    getWebInterviewSettings() {
        return this.http.get(`${this.base}/WebInterviewSettings`)
    }
    setWebInterviewSettings(allowEmails) {
        return this.http.post(`${this.base}/WebInterviewSettings`, {
            allowEmails: allowEmails,
        })
    }
}

class HttpUtil {
    getCsrfCookie() {
        var name = 'CSRF-TOKEN='
        var decodedCookie = decodeURIComponent(document.cookie)
        var ca = decodedCookie.split(';')
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i]
            while (c.charAt(0) == ' ') {
                c = c.substring(1)
            }
            if (c.indexOf(name) == 0) {
                return c.substring(name.length, c.length)
            }
        }
        return ''
    }
}

class HqApiClient {
    constructor(basePath, workspace) {
        this.basePath = basePath
        this.workspace = workspace
        this.http = axios.create({
            baseURL: basePath,
        })
    }

    Questionnaire(questionnaireId, version) {
        return new QuestionnaireApi(questionnaireId, version, this.http)
    }

    get Report() {
        return new Reports(this.http)
    }

    get Users() {
        return new Users(this.http)
    }

    get Assignments() {
        return new AssignmentsApi(this.http)
    }

    get MapDashboard() {
        return new MapDashboard(this.http)
    }

    get WebInterviewSettings() {
        return new WebInterviewSettingsApi(this.http)
    }

    get ExportSettings() {
        return new ExportSettings(this.http)
    }

    get AdminSettings() {
        return new AdminSettings(this.http)
    }

    get ControlPanel() {
        return new ControlPanel(this.http)
    }

    get Util() {
        return new HttpUtil()
    }

    get Workspaces() {
        return new Workspaces(this.http)
    }

    get InterviewsPublicApi() {
        return new InterviewsPublicApi(this.http)
    }

    get UsersManagement() {
        var self = this
        return {
            list() {
                return self.basePath + 'UsersManagement/List'
            },
        }
    }

    workspacePath(workspace) {
        return this.basePath.replace(this.workspace, workspace)
    }
}

/*  the Plugin */
export default {
    install: function (vue) {
        const instance = new HqApiClient(vue.$config.apiBasePath || vue.$config.basePath, vue.$config.workspace)

        // /*  expose a global API method  */
        Object.defineProperty(vue, '$hq', {
            get() {
                return instance
            },
        })

        /*  expose a local API method  */
        Object.defineProperty(vue.prototype, '$hq', {
            get() {
                return instance
            },
        })
    },
}
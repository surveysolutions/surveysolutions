import InterviewersAndDevices from "./InterviewersAndDevices"
import StatusDuration from "./StatusDuration"

export default class ReportComponent {
    get routes() {
        return [{
            path: '/Reports/InterviewersAndDevices', component: InterviewersAndDevices
        }, {
            path: '/Reports/StatusDuration', component: StatusDuration
        }]
    }
}
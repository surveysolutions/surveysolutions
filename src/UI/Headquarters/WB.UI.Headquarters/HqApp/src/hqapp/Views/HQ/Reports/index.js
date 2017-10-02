import InterviewersAndDevices from "./InterviewersAndDevices"
import StatusDuration from "./StatusDuration"

export default {
    routes: [{
        path: '/Reports/InterviewersAndDevices', component: InterviewersAndDevices
    }, {
        path: '/Reports/StatusDuration', component: StatusDuration
    }]
}
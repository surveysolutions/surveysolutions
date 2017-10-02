import Assignments from "./Assignments"
import Interviews from "./Interviews"

export default {
    routes: [{
        path: '/InterviewerHq/CreateNew', component: Assignments
    }, {
        path: '/InterviewerHq/Rejected',  component: Interviews
    }, {
        path: '/InterviewerHq/Completed', component: Interviews
    }, {
        path: '/InterviewerHq/Started',   component: Interviews
    }]
}
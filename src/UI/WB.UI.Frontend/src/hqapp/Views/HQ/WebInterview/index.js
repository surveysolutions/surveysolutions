import Start from "./Start"
import Finish from "./Finish"
import Resume from "./Resume"
import OutdatedBrowser from "./OutdatedBrowser"
import Error from "./Error"

export default class ProfileComponent {
    get routes() {
        return [
            {
                path: '/WebInterview/:invitationId/Start', component: Start
            },
            {
                path: '/WebInterview/:interviewId/Resume', component: Resume
            },
            {
                path: '/WebInterview/:interviewId/Finish', component: Finish
            },
            {
                path: '/WebInterview/OutdatedBrowser', component: OutdatedBrowser
            },
            {
                path: '/WebInterview/Error', component: Error
            }
        ]
    }
}

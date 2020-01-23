import Start from "./Start"
import Finish from "./Finish"
import Resume from "./Resume"
import OutdatedBrowser from "./OutdatedBrowser"
import Error from "./Error"

export default class ProfileComponent {
    get routes() {
        return [
            {
                path: '/WebInterview/Start/:interviewerId', component: Start
            },
            {
                path: '/WebInterview/Finish/:interviewerId', component: Finish
            },
            {
                path: '/WebInterview/Resume/:interviewerId', component: Resume
            },
            {
                path: '/WebInterview/OutdatedBrowser/:interviewerId', component: OutdatedBrowser
            },
            {
                path: '/WebInterview/Error/:interviewerId', component: Error
            }
        ]
    }
}

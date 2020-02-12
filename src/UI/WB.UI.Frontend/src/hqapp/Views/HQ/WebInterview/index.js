import Start from './Start'
import Finish from './Finish'
import Resume from './Resume'
import OutdatedBrowser from './OutdatedBrowser'
import Error from './Error'

export default class ProfileComponent {
    get routes() {
        return [
            {
                path: '/WebInterview/:invitationId/Start',
                component: Start,
            },
            {
                path: '/WebInterview/Resume/:interviewId',
                component: Resume,
            },
            {
                path: '/WebInterview/Finish/:interviewId',
                component: Finish,
            },
            {
                path: '/WebInterview/OutdatedBrowser',
                component: OutdatedBrowser,
            },
            {
                path: '/WebInterview/Error',
                component: Error,
                name: 'WebInterviewError',
            },
        ]
    }
}

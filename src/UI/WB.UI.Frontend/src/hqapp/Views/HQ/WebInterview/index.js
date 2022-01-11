import Start from './Start'
import Finish from './Finish'
import Resume from './Resume'
import Link from './Link'
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
                path: '/WebInterview/Link/:id/:interviewId',
                component: Link,
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

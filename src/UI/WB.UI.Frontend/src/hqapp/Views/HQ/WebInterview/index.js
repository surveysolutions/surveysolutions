const Start = () => import('./Start')
const Finish = () => import('./Finish')
const Resume = () => import('./Resume')
const Link = () => import('./Link')
const OutdatedBrowser = () => import('./OutdatedBrowser')
const Error = () => import('./Error')

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

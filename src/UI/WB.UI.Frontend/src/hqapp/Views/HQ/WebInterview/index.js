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
                beforeEnter: (to, from, next) => {
                    if (window.CONFIG.model.errorMessage)
                        next('/WebInterview/Error')
                    else
                        next()
                },
            },
            {
                path: '/WebInterview/Resume/:interviewId',
                component: Resume,
                beforeEnter: (to, from, next) => {
                    if (window.CONFIG.model.errorMessage)
                        next('/WebInterview/Error')
                    else
                        next()
                },
            },
            {
                path: '/WebInterview/Finish/:interviewId',
                component: Finish,
                beforeEnter: (to, from, next) => {
                    if (window.CONFIG.model.errorMessage)
                        next('/WebInterview/Error')
                    else
                        next()
                },
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

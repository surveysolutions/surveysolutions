const Settings = () => import('./Settings')
const SendInvitations = () => import('./SendInvitations')
const SendInvitationsProgress = () => import('./SendInvitationsProgress')

export default class WebInterviewSetupComponent {
    get routes() {
        return [
            {
                path: '/WebInterviewSetup/Settings/:id',
                component: Settings,
            },
            {
                path: '/WebInterviewSetup/SendInvitations/:id',
                component: SendInvitations,
            },
            {
                path: '/WebInterviewSetup/EmailDistributionProgress',
                component: SendInvitationsProgress,
            },
        ]
    }
}

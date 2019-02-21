import Start from "./Start"
import Started from "./Started"
import SendInvitations from "./SendInvitations"


export default class WebInterviewSetupComponent {
    get routes() {
        return [
        {
            path: '/WebInterviewSetup/Start/:id', 
            component: Start
        },
        {
            path: '/WebInterviewSetup/Started/:id', 
            component: Started
        },
        {
            path: '/WebInterviewSetup/SendInvitations/:id', 
            component: SendInvitations
        }
        ]
    }
}

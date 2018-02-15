import Start from "./Start"
import Started from "./Started"

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
        }
        ]
    }
}

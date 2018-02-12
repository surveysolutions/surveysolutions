import Start from "./Start"

export default class WebInterviewSetupComponent {
    get routes() {
        return [{
            path: '/WebInterviewSetup/Start/:id', 
            component: Start
        }]
    }
}

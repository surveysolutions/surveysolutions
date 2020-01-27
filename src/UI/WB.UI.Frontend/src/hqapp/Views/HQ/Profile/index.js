import Profile from "./Profile"

export default class ProfileComponent {
    get routes() {
        return [{
            path: '/Interviewer/Profile/:interviewerId', component: Profile
        }]
    }
}

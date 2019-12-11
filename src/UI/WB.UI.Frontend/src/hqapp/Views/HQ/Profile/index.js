import Profile from "./Profile"
import Vue from "vue"

export default class ProfileComponent {
    get routes() {
        return [{
            path: '/Interviewer/Profile/:interviewerId', component: Profile
        }]
    }
}

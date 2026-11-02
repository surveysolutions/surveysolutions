const Profile = () => import('./Profile')
const InterviewerAuditLog = () => import('./InterviewerAuditLog')

export default class ProfileComponent {
    get routes() {
        return [{
            path: '/Interviewer/Profile', component: Profile,
        }, {
            path: '/Interviewer/Profile/:interviewerId', component: Profile,
        }, {
            path: '/InterviewerAuditLog/Index/:interviewerId', component: InterviewerAuditLog,
        }]
    }
}

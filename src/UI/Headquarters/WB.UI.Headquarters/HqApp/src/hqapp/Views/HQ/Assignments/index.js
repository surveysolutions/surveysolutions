import Assignments from "./Assignments"

export default class AssignmentsComponent {
    get routes() {
        return [{
            path: '/Assignments/', component: Assignments
        }]
    }
}
import InterviewersAndDevices from "./InterviewersAndDevices";
import StatusDuration from "./StatusDuration";
import MapReport from "./MapReport";

const store = {
    state: {
        
    },
    actions: {
        
    },
    mutations: {
    }
};

export default class ReportComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [
            {
                path: "/Reports/InterviewersAndDevices",
                component: InterviewersAndDevices
            },
            {
                path: "/Reports/StatusDuration",
                component: StatusDuration
            },
            {
                path: "/Reports/MapReport",
                component: MapReport
            }
        ];
    }

    get modules() {
        return { reports: store };
    }    
}

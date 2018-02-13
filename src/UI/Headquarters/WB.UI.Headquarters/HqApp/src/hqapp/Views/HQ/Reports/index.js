import InterviewersAndDevices from "./InterviewersAndDevices"
import StatusDuration from "./StatusDuration"
import MapReport from "./MapReport"

const store = {
    state: {
        exportUrls: {
            excel: "",
            csv: "",
            tab: ""
        }
    },
    actions: {
        setExportUrls(context, urls) {
            context.commit("SET_EXPORT_URLS", urls);
        }
    },
    mutations: {
        SET_EXPORT_URLS(state, urls) {
            state.exportUrls.excel = urls.excel;
            state.exportUrls.csv = urls.csv;
            state.exportUrls.tab = urls.tab;
        }
    }
}

export default class ReportComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [{
            path: '/Reports/InterviewersAndDevices', component: InterviewersAndDevices
        }, {
            path: '/Reports/StatusDuration', component: StatusDuration
        },
        {
            path: '/Reports/MapReport', component: MapReport
        }]
    }

    get modules() { return { reports: store }}
}

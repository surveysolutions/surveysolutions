import InterviewersAndDevices from "./InterviewersAndDevices"
import StatusDuration from "./StatusDuration"

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
        const beforeEnter = (from, to, next) => this.beforeEnter(from, to, next);
        
        const routes =  [{
            path: '/Reports/InterviewersAndDevices', component: InterviewersAndDevices
        }, {
            path: '/Reports/StatusDuration', component: StatusDuration
        }]

        routes.forEach((path) => path.beforeEnter = beforeEnter);

        return routes;
    }

    async beforeEnter(to, from, next) {
        if (this.rootStore.state[this.moduleName] == null) {
            this.rootStore.registerModule(this.moduleName, store)
        }
        next();
    }

    get moduleName() { return "reports" }
}
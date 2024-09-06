import Export from './Export'
import store from './export.store.js'

export default class ExportComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        return [{
            path: '/DataExport/New',
            component: Export,
        },
        ]
    }

    get modules() {
        return {
            export: store,
        }
    }
}

const EmailProviders = () => import('./EmailProviders')
const TabletLogs = () => import('./TabletLogs')
const Settings = () => import('./Settings')
const AuditLog = () => import('./AuditLog')
const TabletInfos = () => import('./TabletInfos')
const InterviewPackages = () => import('./InterviewPackages')

export default class AdminComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        return [{
            path: '/Settings/EmailProviders',
            component: EmailProviders,
        },
        {
            path: '/Diagnostics/Logs',
            component: TabletLogs,
        },
        {
            path: '/Diagnostics/AuditLog',
            component: AuditLog,
        },
        {
            path: '/Diagnostics/TabletInfos',
            component: TabletInfos,
        },
        {
            path: '/Settings',
            component: Settings,
        },
        {
            path: '/Diagnostics/InterviewPackages',
            component: InterviewPackages,
        },
        ]
    }
}

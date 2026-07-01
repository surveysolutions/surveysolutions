const LoginToDesigner = () => import('./LoginToDesigner')
const Import = () => import('./Import')
const ImportMode = () => import('./ImportMode')

export default class Template {
    get routes() {
        return [{
            path: '/Template/LoginToDesigner',
            component: LoginToDesigner,
        },
        {
            path: '/Template/Import',
            component: Import,
        },
        {
            path: '/Template/ImportMode/:questionnaireId',
            component: ImportMode,
        },
        ]
    }
}
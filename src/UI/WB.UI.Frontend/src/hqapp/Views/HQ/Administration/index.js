const AdministrationLayout = () => import('./AdministrationLayout')
const Diagnostics = () => import('./Diagnostics')

export default class MapComponent {
    get routes() {
        return [
            {
                path: '/Administration',
                component: AdministrationLayout,
                children: [
                    {
                        path: 'Diagnostics',
                        component: Diagnostics,
                    },
                ],
            },
        ]
    }
}

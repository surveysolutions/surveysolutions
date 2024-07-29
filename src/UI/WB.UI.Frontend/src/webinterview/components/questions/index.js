// tslint:disable-next-line:ordered-imports

export function registerQuestionComponents(vue) {

    vue.component('Audio', () => import('./Audio'))
    vue.component('CategoricalMulti', () => import('./CategoricalMulti'))
    vue.component('MultiCombobox', () => import('./MultiCombobox'))
    vue.component('CategoricalSingle', () => import('./CategoricalSingle'))
    vue.component('CategoricalYesNo', () => import('./CategoricalYesNo'))
    vue.component('Combobox', () => import('./Combobox'))
    vue.component('DateTime', () => import('./DateTime'))
    vue.component('Double', () => import('./Double'))
    vue.component('Gps', () => import('./Gps'))
    vue.component('Group', () => import('./Group'))
    vue.component('GroupTitle', () => import('./GroupTitle'))
    vue.component('Integer', () => import('./Integer'))
    vue.component('LinkedMulti', () => import('./LinkedMulti'))
    vue.component('LinkedSingle', () => import('./LinkedSingle'))
    vue.component('Multimedia', () => import('./Multimedia'))
    vue.component('NavigationButton', () => import('./NavigationButton'))
    vue.component('QRBarcode', () => import('./QRBarcode'))
    vue.component('StaticText', () => import('./StaticText'))
    vue.component('TextList', () => import('./TextList'))
    vue.component('TextQuestion', () => import('./TextQuestion'))
    vue.component('Area', () => import('./Geography'))
    vue.component('Variable', () => import('./Variable'))
    vue.component('TableRoster', () => import('./TableRoster'))
    vue.component('MatrixRoster', () => import('./MatrixRoster'))
    vue.component('Unsupported', () => import('./Unsupported'))
    vue.component('ReadonlyQuestion', () => import('./ReadonlyQuestion'))

    vue.component('TableRoster_TextQuestion', () => import('./TableRoster.TextQuestion'))
    vue.component('TableRoster_Integer', () => import('./TableRoster.Integer'))
    vue.component('TableRoster_Double', () => import('./TableRoster.Double'))
    vue.component('TableRoster_Unsupported', () => import('./TableRoster.Unsupported'))

    vue.component('MatrixRoster_CategoricalSingle', () => import('./MatrixRoster.CategoricalSingle'))
    vue.component('MatrixRoster_CategoricalMulti', () => import('./MatrixRoster.CategoricalMulti'))
    vue.component('MatrixRoster_Unsupported', () => import('./MatrixRoster.Unsupported'))

    vue.component('wb-question', () => import('./Question'))
    vue.component('wb-humburger', () => import('./ui/humburger'))
    vue.component('wb-typeahead', () => import('./ui/typeahead'))
}
export const GroupStatus = {
    Disabled: 'Disabled',
    NotStarted: 'NotStarted',
    Started: 'Started',
    StartedInvalid: 'StartedInvalid',
    Completed: 'Completed',
    CompletedInvalid: 'CompletedInvalid',
}

export const ButtonType = {
    Start: 'Start',
    Next: 'Next',
    Parent: 'Parent',
    Complete: 'Complete',
}


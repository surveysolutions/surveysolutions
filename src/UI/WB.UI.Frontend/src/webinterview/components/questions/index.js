// tslint:disable-next-line:ordered-imports

import Vue from 'vue'

Vue.component('Audio', () => import(/* webpackChunkName: "questions" */'./Audio'))
Vue.component('CategoricalMulti', () => import(/* webpackChunkName: "questions" */'./CategoricalMulti'))
Vue.component('MultiCombobox', () => import(/* webpackChunkName: "questions" */'./MultiCombobox'))
Vue.component('CategoricalSingle', () => import(/* webpackChunkName: "questions" */'./CategoricalSingle'))
Vue.component('CategoricalYesNo', () => import(/* webpackChunkName: "questions" */'./CategoricalYesNo'))
Vue.component('Combobox', () => import(/* webpackChunkName: "questions" */'./Combobox'))
Vue.component('DateTime', () => import(/* webpackChunkName: "questions" */'./DateTime'))
Vue.component('Double', () => import(/* webpackChunkName: "questions" */'./Double'))
Vue.component('Gps', () => import(/* webpackChunkName: "questions" */'./Gps'))
Vue.component('Group', () => import(/* webpackChunkName: "questions" */'./Group'))
Vue.component('GroupTitle', () => import(/* webpackChunkName: "questions" */'./GroupTitle'))
Vue.component('Integer', () => import(/* webpackChunkName: "questions" */'./Integer'))
Vue.component('LinkedMulti', () => import(/* webpackChunkName: "questions" */'./LinkedMulti'))
Vue.component('LinkedSingle', () => import(/* webpackChunkName: "questions" */'./LinkedSingle'))
Vue.component('Multimedia', () => import(/* webpackChunkName: "questions" */'./Multimedia'))
Vue.component('NavigationButton', () => import(/* webpackChunkName: "questions" */'./NavigationButton'))
Vue.component('QRBarcode', () => import(/* webpackChunkName: "questions" */'./QRBarcode'))
Vue.component('StaticText', () => import(/* webpackChunkName: "questions" */'./StaticText'))
Vue.component('TextList', () => import(/* webpackChunkName: "questions" */'./TextList'))
Vue.component('TextQuestion', () => import(/* webpackChunkName: "questions" */'./TextQuestion'))
Vue.component('Area', () => import(/* webpackChunkName: "questions" */'./Geography'))
Vue.component('Variable', () => import(/* webpackChunkName: "questions" */'./Variable'))
Vue.component('TableRoster', () => import(/* webpackChunkName: "table-roster" */'./TableRoster'))
Vue.component('MatrixRoster', () => import(/* webpackChunkName: "matrix-roster" */'./MatrixRoster'))
Vue.component('Unsupported', () => import(/* webpackChunkName: "questions" */'./Unsupported'))

Vue.component('TableRoster_TextQuestion', () => import(/* webpackChunkName: "table-roster" */'./TableRoster.TextQuestion'))
Vue.component('TableRoster_Integer', () => import(/* webpackChunkName: "table-roster" */'./TableRoster.Integer'))
Vue.component('TableRoster_Double', () => import(/* webpackChunkName: "table-roster" */'./TableRoster.Double'))
Vue.component('TableRoster_Unsupported', () => import(/* webpackChunkName: "table-roster" */'./TableRoster.Unsupported'))

Vue.component('MatrixRoster_CategoricalSingle', () => import(/* webpackChunkName: "matrix-roster" */'./MatrixRoster.CategoricalSingle'))
Vue.component('MatrixRoster_CategoricalMulti', () => import(/* webpackChunkName: "matrix-roster" */'./MatrixRoster.CategoricalMulti'))
Vue.component('MatrixRoster_Unsupported', () => import(/* webpackChunkName: "matrix-roster" */'./MatrixRoster.Unsupported'))

Vue.component('wb-question', () => import( /* webpackChunkName: "questions" */ './Question'))
Vue.component('wb-humburger', () => import( /* webpackChunkName: "questions" */ './ui/humburger'))
Vue.component('wb-typeahead', () => import( /* webpackChunkName: "questions" */ './ui/typeahead'))

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


// tslint:disable-next-line:ordered-imports

import Vue from 'vue'

Vue.component('Audio', () => import( './Audio'))
Vue.component('CategoricalMulti', () => import( './CategoricalMulti'))
Vue.component('MultiCombobox', () => import( './MultiCombobox'))
Vue.component('CategoricalSingle', () => import( './CategoricalSingle'))
Vue.component('CategoricalYesNo', () => import( './CategoricalYesNo'))
Vue.component('Combobox', () => import( './Combobox'))
Vue.component('DateTime', () => import( './DateTime'))
Vue.component('Double', () => import( './Double'))
Vue.component('Gps', () => import( './Gps'))
Vue.component('Group', () => import( './Group'))
Vue.component('GroupTitle', () => import( './GroupTitle'))
Vue.component('Integer', () => import( './Integer'))
Vue.component('LinkedMulti', () => import( './LinkedMulti'))
Vue.component('LinkedSingle', () => import( './LinkedSingle'))
Vue.component('Multimedia', () => import( './Multimedia'))
Vue.component('NavigationButton', () => import( './NavigationButton'))
Vue.component('QRBarcode', () => import( './QRBarcode'))
Vue.component('StaticText', () => import( './StaticText'))
Vue.component('TextList', () => import( './TextList'))
Vue.component('TextQuestion', () => import( './TextQuestion'))
Vue.component('Area', () => import( './Geography'))
Vue.component('Variable', () => import( './Variable'))
Vue.component('TableRoster', () => import( './TableRoster'))
Vue.component('MatrixRoster', () => import( './MatrixRoster'))
Vue.component('Unsupported', () => import( './Unsupported'))

Vue.component('TableRoster_TextQuestion', () => import( './TableRoster.TextQuestion'))
Vue.component('TableRoster_Integer', () => import( './TableRoster.Integer'))
Vue.component('TableRoster_Double', () => import( './TableRoster.Double'))
Vue.component('TableRoster_Unsupported', () => import( './TableRoster.Unsupported'))

Vue.component('MatrixRoster_CategoricalSingle', () => import( './MatrixRoster.CategoricalSingle'))
Vue.component('MatrixRoster_CategoricalMulti', () => import( './MatrixRoster.CategoricalMulti'))
Vue.component('MatrixRoster_Unsupported', () => import( './MatrixRoster.Unsupported'))

Vue.component('wb-question', () => import(   './Question'))
Vue.component('wb-humburger', () => import(   './ui/humburger'))
Vue.component('wb-typeahead', () => import(   './ui/typeahead'))

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


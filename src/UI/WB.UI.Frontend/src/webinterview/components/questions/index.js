import { defineAsyncComponent } from 'vue';

// tslint:disable-next-line:ordered-imports

export function registerQuestionComponents(vue) {

    vue.component('Audio', defineAsyncComponent(() => import('./Audio')))
    vue.component('CategoricalMulti', defineAsyncComponent(() => import('./CategoricalMulti')))
    vue.component('MultiCombobox', defineAsyncComponent(() => import('./MultiCombobox')))
    vue.component('CategoricalSingle', defineAsyncComponent(() => import('./CategoricalSingle')))
    vue.component('CategoricalYesNo', defineAsyncComponent(() => import('./CategoricalYesNo')))
    vue.component('Combobox', defineAsyncComponent(() => import('./Combobox')))
    vue.component('DateTime', defineAsyncComponent(() => import('./DateTime')))
    vue.component('Double', defineAsyncComponent(() => import('./Double')))
    vue.component('Gps', defineAsyncComponent(() => import('./Gps')))
    vue.component('Group', defineAsyncComponent(() => import('./Group')))
    vue.component('GroupTitle', defineAsyncComponent(() => import('./GroupTitle')))
    vue.component('Integer', defineAsyncComponent(() => import('./Integer')))
    vue.component('LinkedMulti', defineAsyncComponent(() => import('./LinkedMulti')))
    vue.component('LinkedSingle', defineAsyncComponent(() => import('./LinkedSingle')))
    vue.component('Multimedia', defineAsyncComponent(() => import('./Multimedia')))
    vue.component('NavigationButton', defineAsyncComponent(() => import('./NavigationButton')))
    vue.component('QRBarcode', defineAsyncComponent(() => import('./QRBarcode')))
    vue.component('StaticText', defineAsyncComponent(() => import('./StaticText')))
    vue.component('TextList', defineAsyncComponent(() => import('./TextList')))
    vue.component('TextQuestion', defineAsyncComponent(() => import('./TextQuestion')))
    vue.component('Area', defineAsyncComponent(() => import('./Geography')))
    vue.component('Variable', defineAsyncComponent(() => import('./Variable')))
    vue.component('TableRoster', defineAsyncComponent(() => import('./TableRoster')))
    vue.component('MatrixRoster', defineAsyncComponent(() => import('./MatrixRoster')))
    vue.component('Unsupported', defineAsyncComponent(() => import('./Unsupported')))
    vue.component('ReadonlyQuestion', defineAsyncComponent(() => import('./ReadonlyQuestion')))

    vue.component('TableRoster_TextQuestion', defineAsyncComponent(() => import('./TableRoster.TextQuestion')))
    vue.component('TableRoster_Integer', defineAsyncComponent(() => import('./TableRoster.Integer')))
    vue.component('TableRoster_Double', defineAsyncComponent(() => import('./TableRoster.Double')))
    vue.component('TableRoster_Unsupported', defineAsyncComponent(() => import('./TableRoster.Unsupported')))

    vue.component('MatrixRoster_CategoricalSingle', defineAsyncComponent(() => import('./MatrixRoster.CategoricalSingle')))
    vue.component('MatrixRoster_CategoricalMulti', defineAsyncComponent(() => import('./MatrixRoster.CategoricalMulti')))
    vue.component('MatrixRoster_Unsupported', defineAsyncComponent(() => import('./MatrixRoster.Unsupported')))

    vue.component('wb-question', defineAsyncComponent(() => import('./Question')))
    vue.component('wb-humburger', defineAsyncComponent(() => import('./ui/humburger')))
    vue.component('wb-typeahead', defineAsyncComponent(() => import('./ui/typeahead')))
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


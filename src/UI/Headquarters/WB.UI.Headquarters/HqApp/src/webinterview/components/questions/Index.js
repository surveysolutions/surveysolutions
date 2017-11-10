// tslint:disable-next-line:ordered-imports

import Vue from "vue"

Vue.component("Audio",              () => import(/* webpackChunkName: "questions" */"./Audio"))
Vue.component("CategoricalMulti",   () => import(/* webpackChunkName: "questions" */"./CategoricalMulti"))
Vue.component("CategoricalSingle",  () => import(/* webpackChunkName: "questions" */"./CategoricalSingle"))
Vue.component("CategoricalYesNo",   () => import(/* webpackChunkName: "questions" */"./CategoricalYesNo"))
Vue.component("Combobox",           () => import(/* webpackChunkName: "questions" */"./Combobox"))
Vue.component("DateTime",           () => import(/* webpackChunkName: "questions" */"./DateTime"))
Vue.component("Double",             () => import(/* webpackChunkName: "questions" */"./Double"))
Vue.component("Gps",                () => import(/* webpackChunkName: "questions" */"./Gps"))
Vue.component("Group",              () => import(/* webpackChunkName: "questions" */"./Group"))
Vue.component("Integer",            () => import(/* webpackChunkName: "questions" */"./Integer"))
Vue.component("LinkedMulti",        () => import(/* webpackChunkName: "questions" */"./LinkedMulti"))
Vue.component("LinkedSingle",       () => import(/* webpackChunkName: "questions" */"./LinkedSingle"))
Vue.component("Multimedia",         () => import(/* webpackChunkName: "questions" */"./Multimedia"))
Vue.component("NavigationButton",   () => import(/* webpackChunkName: "questions" */"./NavigationButton"))
Vue.component("QRBarcode",          () => import(/* webpackChunkName: "questions" */"./QRBarcode"))
Vue.component("StaticText",         () => import(/* webpackChunkName: "questions" */"./StaticText"))
Vue.component("TextList",           () => import(/* webpackChunkName: "questions" */"./TextList"))
Vue.component("TextQuestion",       () => import(/* webpackChunkName: "questions" */"./TextQuestion"))
Vue.component("Unsupported",        () => import(/* webpackChunkName: "questions" */"./Unsupported"))

Vue.component("wb-question",        () => import( /* webpackChunkName: "questions" */ "./Question"))
Vue.component("wb-humburger",       () => import( /* webpackChunkName: "questions" */ "./ui/humburger"))
Vue.component("wb-typeahead",       () => import( /* webpackChunkName: "questions" */ "./ui/typeahead"))

export const GroupStatus = {
    NotStarted: 1,
    Started: 2,
    Completed: 3,
    Invalid: 4
}

export const ButtonType = {
    Start: 0,
    Next: 1,
    Parent: 2,
    Complete: 3
}


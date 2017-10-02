// tslint:disable-next-line:ordered-imports
import "bootstrap/js/dropdown";
import "jquery-mask-plugin"

import Vue from "vue"

import Audio from "./Audio"
import CategoricalMulti from "./CategoricalMulti"
import CategoricalSingle from "./CategoricalSingle"
import CategoricalYesNo from "./CategoricalYesNo"
import Combobox from "./Combobox"
import DateTime from "./DateTime"
import Double from "./Double"
import Gps from "./Gps"
import Group from "./Group"
import Integer from "./Integer"
import LinkedMulti from "./LinkedMulti"
import LinkedSingle from "./LinkedSingle"
import Multimedia from "./Multimedia"
import NavigationButton from "./NavigationButton"
import QRBarcode from "./QRBarcode"
import Question from "./Question"
import StaticText from "./StaticText"
import TextList from "./TextList"
import TextQuestion from "./TextQuestion"
import Humburger from "./ui/humburger"
import Unsupported from "./Unsupported"
import Typeahead from "./ui/typeahead"

Vue.component("Audio", Audio)
Vue.component("CategoricalMulti", CategoricalMulti)
Vue.component("CategoricalSingle", CategoricalSingle)
Vue.component("CategoricalYesNo", CategoricalYesNo)
Vue.component("Combobox", Combobox)
Vue.component("DateTime", DateTime)
Vue.component("Double", Double)
Vue.component("Gps", Gps)
Vue.component("Group", Group)
Vue.component("Integer", Integer)
Vue.component("LinkedMulti", LinkedMulti)
Vue.component("LinkedSingle", LinkedSingle)
Vue.component("Multimedia", Multimedia)
Vue.component("NavigationButton", NavigationButton)
Vue.component("QRBarcode", QRBarcode)
Vue.component("StaticText", StaticText)
Vue.component("TextList", TextList)
Vue.component("TextQuestion", TextQuestion)
Vue.component("Unsupported", Unsupported)
Vue.component("wb-question", Question)
Vue.component("wb-humburger", Humburger)
Vue.component("wb-typeahead", Typeahead)

export const GroupStatus = {
    Completed: 1,
    Invalid: -1,
    Other: 0,
}

export const ButtonType = {
    Start: 0,
    Next: 1,
    Parent: 2,
    Complete: 3
}

export const DateFormats = {
    dateTime: 'YYYY-MM-DD HH:mm:ss',
    date: 'YYYY-MM-DD'
}

// tslint:disable-next-line:ordered-imports
import * as jQuery from "jquery"
const $ = (window as any).$ = (window as any).jQuery = jQuery
import "bootstrap-sass/assets/javascripts/bootstrap/dropdown"
import "bootstrap-sass/assets/javascripts/bootstrap/modal"

import "jquery-mask-plugin"

import * as vue from "vue"

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

vue.component("CategoricalMulti", CategoricalMulti)
vue.component("CategoricalSingle", CategoricalSingle)
vue.component("CategoricalYesNo", CategoricalYesNo)
vue.component("DateTime", DateTime)
vue.component("Double", Double)
vue.component("Gps", Gps)
vue.component("Group", Group)
vue.component("Integer", Integer)
vue.component("LinkedMulti", LinkedMulti)
vue.component("LinkedSingle", LinkedSingle)
vue.component("Multimedia", Multimedia)
vue.component("NavigationButton", NavigationButton)
vue.component("QRBarcode", QRBarcode)
vue.component("StaticText", StaticText)
vue.component("TextList", TextList)
vue.component("TextQuestion", TextQuestion)
vue.component("Unsupported", Unsupported)
vue.component("wb-question", Question)
vue.component("wb-humburger", Humburger)
vue.component("Combobox", Combobox)

vue.component("wb-typeahead", (resolve, reject) => {
     require.ensure(["./ui/typeahead"], r => {
        resolve(require("./ui/typeahead").default)
    }, "libs")
})

vue.component("vue-flatpickr", (resolve, reject) => {
     require.ensure(["vue-flatpickr"], r => {
        const flatpick = require("vue-flatpickr")
        resolve(flatpick.default)
    }, "libs")
})

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

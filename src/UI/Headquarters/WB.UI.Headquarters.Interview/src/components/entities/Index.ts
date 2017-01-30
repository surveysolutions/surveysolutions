declare const require: any

// tslint:disable-next-line:ordered-imports
import * as jQuery from "jquery"
const $ = (window as any).$ = (window as any).jQuery = jQuery
import "bootstrap-sass/assets/javascripts/bootstrap/modal"

import "jquery-mask-plugin"

import * as vue from "vue"

import CategoricalMulti from "./CategoricalMulti"
import CategoricalSingle from "./CategoricalSingle"
import Combobox from "./Combobox"
import DateTime from "./DateTime"
import Double from "./Double"
import Group from "./Group"
import Integer from "./Integer"
import NavigationButton from "./NavigationButton"
import Question from "./Question"
import StaticText from "./StaticText"
import TextQuestion from "./TextQuestion"

vue.component("DateTime", DateTime)
vue.component("Integer", Integer)
vue.component("Double", Double)
vue.component("TextQuestion", TextQuestion)
vue.component("CategoricalSingle", CategoricalSingle)
vue.component("Combobox", Combobox)
vue.component("CategoricalMulti", CategoricalMulti)
vue.component("wb-question", Question)
vue.component("StaticText", StaticText)
vue.component("Group", Group)
vue.component("NavigationButton", NavigationButton)

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

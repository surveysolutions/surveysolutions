declare const require: any

// tslint:disable-next-line:ordered-imports
import * as jQuery from "jquery"
const $ = (window as any).$ = (window as any).jQuery = jQuery
import "bootstrap-sass/assets/javascripts/bootstrap/modal"

import "jquery-mask-plugin"
// tslint:disable-next-line:ordered-imports
import "autoNumeric"

import * as vue from "vue"

import CategoricalMulti from "./CategoricalMulti"
import CategoricalSingle from "./CategoricalSingle"
import DateTime from "./DateTime"
import Double from "./Double"
import Integer from "./Integer"
import Question from "./Question"
import StaticText from "./StaticText"
import TextQuestion from "./TextQuestion"


import Group from "./Group"

vue.component("DateTime", DateTime)
vue.component("Integer", Integer)
vue.component("Double", Double)
vue.component("TextQuestion", TextQuestion)
vue.component("CategoricalSingle", CategoricalSingle)
vue.component("CategoricalMulti", CategoricalMulti)
vue.component("wb-question", Question)
vue.component("StaticText", StaticText)
vue.component("Group", Group)

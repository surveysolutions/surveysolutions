import * as vue from "vue"

import CategoricalMulti from "./CategoricalMulti"
import CategoricalSingle from "./CategoricalSingle"
import DateTime from "./DateTime"
import Double from "./Double"
import Integer from "./Integer"
import Question from "./Question"
import TextQuestion from "./TextQuestion"

import StaticText from "./StaticText"

vue.component("DateTime", DateTime)
vue.component("Integer", Integer)
vue.component("Double", Double)
vue.component("TextQuestion", TextQuestion)
vue.component("CategoricalSingle", CategoricalSingle)
vue.component("CategoricalMulti", CategoricalMulti)

vue.component("wb-question", Question)

vue.component("StaticText", StaticText)

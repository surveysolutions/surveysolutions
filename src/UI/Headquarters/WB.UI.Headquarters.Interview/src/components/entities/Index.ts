import * as vue from "vue"

import CategoricalSingle from "./CategoricalSingle"
import DateTime from "./DateTime"
import Integer from "./Integer"
import Question from "./Question"
import TextQuestion from "./TextQuestion"

import StaticText from "./StaticText"

vue.component("DateTime", DateTime)
vue.component("Integer", Integer)
vue.component("TextQuestion", TextQuestion)
vue.component("CategoricalSingle", CategoricalSingle)
vue.component("wb-question", Question)

vue.component("StaticText", StaticText)

import Vue from "vue"

import Instructions from "./Instructions"
import Progress from "./Progress"
import RemoveAnswer from "./RemoveAnswer"
import Title from "./Title"
import Validation from "./Validation"

Vue.component("wb-instructions", Instructions)
Vue.component("wb-title", Title)
Vue.component("wb-validation", Validation)
Vue.component("wb-remove-answer", RemoveAnswer)
Vue.component("wb-progress", Progress)

Vue.component("wb-attachment", (resolve, reject) => {
     require.ensure(["./Attachment"], r => {
        resolve(require("./Attachment").default)
    }, "libs")
})

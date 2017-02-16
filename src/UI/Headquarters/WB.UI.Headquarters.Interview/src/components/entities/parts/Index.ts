import * as vue from "vue"

import Instructions from "./Instructions"
import Progress from "./Progress"
import RemoveAnswer from "./RemoveAnswer"
import Title from "./Title"
import Validation from "./Validation"

vue.component("wb-instructions", Instructions)
vue.component("wb-title", Title)
vue.component("wb-validation", Validation)
vue.component("wb-remove-answer", RemoveAnswer)
vue.component("wb-progress", Progress)

vue.component("wb-attachment", (resolve, reject) => {
     require.ensure(["./Attachment"], r => {
        resolve(require("./Attachment").default)
    }, "libs")
})

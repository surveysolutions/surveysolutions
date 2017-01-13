import * as vue from "vue"

import ActionButtons from "./ActionButtons"
import Attachment from "./Attachment"
import Instructions from "./Instructions"
import RemoveAnswer from "./RemoveAnswer"
import Title from "./Title"
import Validation from "./Validation"

vue.component("wb-instructions", Instructions)
vue.component("wb-title", Title)
vue.component("wb-validation", Validation)
vue.component("wb-remove-answer", RemoveAnswer)
vue.component("wb-attachment", Attachment)
vue.component("wb-actionButtons", ActionButtons)

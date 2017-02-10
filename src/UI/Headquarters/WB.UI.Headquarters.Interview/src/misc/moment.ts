// import * as moment from "moment"

// const result = new Promise()

declare var require: any

const moment = new Promise<any>(promise => {
    require.ensure(["moment"], res => {
        promise(require("moment"))
    }, "time")
})

export default moment

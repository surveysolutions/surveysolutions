import * as forEach from "lodash/foreach"
import Vue from "vue"

Vue.directive("dateTimeFormatting", {
    bind: (el, binding) => {
        const timeElements = el.getElementsByTagName("time")
        forEach(timeElements, timeElement => {
            const date = new Date(timeElement.getAttribute("datetime"))
            timeElement.innerHTML = date.toLocaleString()
        })
    },
    update: (el, binding) => {
        const timeElements = el.getElementsByTagName("time")
        forEach(timeElements, timeElement => {
            const date = new Date(timeElement.getAttribute("datetime"))
            timeElement.innerHTML = date.toLocaleString()
        })
    },
})

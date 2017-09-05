import * as forEach from "lodash/foreach"
import Vue from "vue"
import * as format from "date-fns/format"
import { DateFormats } from "components/entities"

Vue.directive("dateTimeFormatting", {
    update: (el, binding) => {
        const timeElements = el.getElementsByTagName("time")
        forEach(timeElements, timeElement => {
            const dateTimeAttr = timeElement.getAttribute("datetime")
            if (dateTimeAttr) {
                const dateTime = new Date(dateTimeAttr)
                timeElement.innerHTML =  format(dateTime, DateFormats.dateTime)
            }
            else {
                const date = new Date(timeElement.getAttribute("date"))
                timeElement.innerHTML =  format(date, DateFormats.date)
            }
        })
    },
})

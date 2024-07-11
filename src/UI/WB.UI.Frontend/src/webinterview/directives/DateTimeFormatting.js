import Vue from 'vue'
import { DateFormats } from '~/shared/helpers'
import { forEach } from 'lodash'
import moment from 'moment'

Vue.directive('dateTimeFormatting', (el) => {
    const timeElements = el.getElementsByTagName('time')

    forEach(timeElements, timeElement => {
        const dateTimeAttr = timeElement.getAttribute('datetime')

        if (dateTimeAttr) {
            const dateTime = new Date(dateTimeAttr)
            timeElement.innerHTML = moment(dateTime).format(DateFormats.dateTime)
        }
        else {
            const date = timeElement.getAttribute('date')
            timeElement.innerHTML = moment(date).format(DateFormats.date)
        }
    })
})

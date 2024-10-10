import { DateFormats } from '~/shared/helpers';
import { forEach } from 'lodash';
import moment from 'moment';

export function registerDateTimeFormatting(app) {
    app.directive('dateTimeFormatting', {
        mounted(el) {
            formatTimeElements(el);
        },
        updated(el) {
            formatTimeElements(el);
        }
    });
}

function formatTimeElements(el) {
    const timeElements = el.getElementsByTagName('time');

    forEach(timeElements, (timeElement) => {
        const dateTimeAttr = timeElement.getAttribute('datetime');

        if (dateTimeAttr) {
            const dateTime = new Date(dateTimeAttr);
            timeElement.innerHTML = moment(dateTime).format(DateFormats.dateTime);
        } else {
            const date = timeElement.getAttribute('date');
            timeElement.innerHTML = moment(date).format(DateFormats.date);
        }
    });
}
import { setupErrorHandler } from './errorsHandler';
setupErrorHandler();

import $ from 'jquery';
window.jQuery = window.$ = $;

import 'bootstrap';
import '/Scripts/custom/utils.js';

/*$(function () {
    if (window.moment) {
        moment.locale('@CultureInfo.CurrentUICulture');

        $('time').each(function () {
            var me = $(this);
            var date = me.attr('datetime') || me.attr('time');
            var momentDate = moment
                .utc(date)
                .local()
                .format('MMM DD, YYYY HH:mm');
            me.text(momentDate);
        });
    }
});
*/

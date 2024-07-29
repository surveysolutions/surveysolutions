import $ from 'jquery';

import '/node_modules/jquery-mousewheel/jquery.mousewheel.js';
import PerfectScrollbar from 'perfect-scrollbar';

window.jQuery = window.$ = $;

import moment from '/node_modules/moment/min/moment-with-locales';
//import moment from 'moment';
window.moment = moment;

import '/node_modules/perfect-scrollbar/css/perfect-scrollbar.css';
import '/questionnaire/content/designer-start/bootstrap-custom.less';
import '/Styles/designer-list.less';
import '/Scripts/custom/common.js';

import './simplepage';

$(function () {
    const lists = $('#table-content-holder > .scroller-container');
    if (lists.length > 0) {
        const ps = new PerfectScrollbar(
            '#table-content-holder > .scroller-container'
        );
    }

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

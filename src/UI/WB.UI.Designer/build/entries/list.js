import $ from 'jquery';

import PerfectScrollbar from 'perfect-scrollbar';

window.jQuery = window.$ = $;

import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import localizedFormat from 'dayjs/plugin/localizedFormat';
dayjs.extend(utc);
dayjs.extend(localizedFormat);

import '/node_modules/perfect-scrollbar/css/perfect-scrollbar.css';
import '/questionnaire/content/designer-start/bootstrap-custom.less';
import '/questionnaire/content/designer-start/designer-list.less';
import '/Scripts/custom/common.js';

import './simplepage';

$(function () {
    const lists = $('#table-content-holder > .scroller-container');
    if (lists.length > 0) {
        const ps = new PerfectScrollbar(
            '#table-content-holder > .scroller-container'
        );
    }

    $('time').each(function () {
        var me = $(this);
        var date = me.attr('datetime') || me.attr('time');
        var formatted = dayjs.utc(date).local().format('MMM DD, YYYY HH:mm');
        me.text(formatted);
    });
});

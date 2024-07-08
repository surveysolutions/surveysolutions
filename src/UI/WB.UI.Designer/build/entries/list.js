//import '/node_modules/jquery/dist/jquery.js';
//import $ from '/node_modules/jquery/dist/jquery.js';
import $ from 'jquery';
window.jQuery = window.$ = $;
import '/node_modules/jquery-mousewheel/jquery.mousewheel.js';
//import '/node_modules/perfect-scrollbar/dist/perfect-scrollbar.js';
//import 'perfect-scrollbar';
import perfectScrollbar from 'perfect-scrollbar';
//import perfectScrollbar from '/node_modules/perfect-scrollbar/dist/perfect-scrollbar.js';
//$.perfectScrollbar = perfectScrollbar();
const lists = $('#table-content-holder > .scroller-container');
if (lists.length > 0) {
    perfectScrollbar.initialize(
        $('#table-content-holder > .scroller-container')[0]
    );
}

import '/node_modules/moment/min/moment-with-locales.js';
import '/node_modules/perfect-scrollbar/dist/css/perfect-scrollbar.css';
import '/questionnaire/content/designer-start/bootstrap-custom.less';
import '/Styles/designer-list.less';
import '/Scripts/custom/common.js';

//export default $;

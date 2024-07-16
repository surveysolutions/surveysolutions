//import './simplepage';

//import $ from 'jquery';
//window.jQuery = window.$ = $;

//import 'jquery-ui-dist';

/*import 'jquery.fancytree/dist/skin-lion/ui.fancytree.less'; // CSS or LESS

import { createTree } from 'jquery.fancytree';

import 'jquery.fancytree/dist/modules/jquery.fancytree.edit';
import 'jquery.fancytree/dist/modules/jquery.fancytree.filter';

const tree = createTree('#tree', {
    extensions: ['edit', 'filter'],
});*/

//import 'jquery';
//import $ from 'jquery';

//import 'jquery-ui-dist/jquery-ui.css';
//import 'jquery-ui-dist/jquery-ui';

//import 'jquery-ui';
//import ui from 'jquery-ui-dist/jquery-ui.js';
//$.ui = ui;

//import 'jquery.fancytree';
//import '/Content/plugins/jquery.fancytree-all-deps.min';
//import '/node_modules/jquery.fancytree/dist/jquery.fancytree-all-deps.js';
//import 'jquery.fancytree';
//import '/questionnaire/external/jquery.fancytree.contextMenu.js';
//import '/node_modules/jquery-contextmenu/dist/jquery.contextMenu.js';
//import 'jquery-contextmenu';

import '/node_modules/jquery.fancytree/dist/skin-bootstrap/ui.fancytree.min.css';
import '/node_modules/jquery-contextmenu/dist/jquery.contextMenu.min.css';

import $ from 'jquery';
import 'jquery-ui-dist/jquery-ui';
import 'jquery.fancytree';
//import 'jquery.fancytree/dist/jquery.fancytree-all.min';
import 'jquery.fancytree/dist/modules/jquery.fancytree.edit';
import 'jquery.fancytree/dist/modules/jquery.fancytree.filter';
import 'jquery.fancytree/dist/modules/jquery.fancytree.menu';
import 'jquery.fancytree/dist/modules/jquery.fancytree.glyph';
import '/questionnaire/external/jquery.fancytree.contextMenu.js';
import 'jquery-contextmenu';
import '/Scripts/custom/public-folders.js';
//        "node_modules/jquery-contextmenu/dist/jquery.contextMenu.js",

window.jQuery = window.$ = $;

$(function () {
    /*import('jquery.fancytree/dist/jquery.fancytree-all.min')
        .then(() => {
            console.log('jQuery version:', $.fn.jquery);
            console.log('jQuery UI version:', $.ui.version);
            console.log(
                'Fancytree loaded:',
                typeof $.ui.fancytree !== 'undefined'
            );

            // Ваш код инициализации fancytree
            $('#tree').fancytree({
                source: [
                    { title: 'Node 1', key: '1' },
                    {
                        title: 'Folder 2',
                        folder: true,
                        children: [
                            { title: 'Node 2.1', key: '3' },
                            { title: 'Node 2.2', key: '4' },
                        ],
                    },
                ],
            });
        })
        .catch((error) => {
            console.error('Ошибка загрузки Fancytree:', error);
        });*/
});

window.jQuery = window.$ = $;
/*const $ = require('jquery');
window.jQuery = window.$ = $;
const ui = require('jquery-ui-dist/jquery-ui');
console.log(ui);
const fancytree = require('jquery.fancytree');
//const fancytree = require('jquery.fancytree/dist/modules/jquery.fancytree.ui-deps');
console.log(fancytree);*/
//import { createTree } from 'jquery.fancytree';
//console.log(createTree);

//require('jquery.fancytree/dist/modules/jquery.fancytree.edit');
//require('jquery.fancytree/dist/modules/jquery.fancytree.filter');

//import { createTree } from 'jquery.fancytree';
//console.log(createTree);
//import { createTree } from 'jquery.fancytree/dist/modules/jquery.fancytree.ui-deps';
//import * as fancy from 'jquery.fancytree/dist/modules/jquery.fancytree.ui-deps';
//import 'jquery.fancytree/dist/jquery.fancytree';
//import { createTree } from 'jquery.fancytree';

//console.log(fancy);

debugger;
if (!$.ui) {
    console.error('jQuery UI is not loaded properly.');
} else {
    console.log('jQuery UI loaded successfully.');
}

//import 'jquery.fancytree/dist/modules/jquery.fancytree.edit';
//import 'jquery.fancytree/dist/modules/jquery.fancytree.filter';

/*const tree = createTree('#tree', {
    extensions: ['edit', 'filter'],
    //source: {...},
    //...
});*/

/*$(function () {
    debugger;
    if (!$.ui) {
        console.error('jQuery UI is not loaded properly.');
    } else {
        console.log('jQuery UI loaded successfully.');
    }

    const tree = fancy.createTree('#tree', {
        extensions: ['edit', 'filter'],
        //source: {...},
        //...
    });

    $('#tree').fancytree({
        source: [
            { title: 'Node 1', key: '1' },
            {
                title: 'Folder 2',
                key: '2',
                folder: true,
                children: [
                    { title: 'Node 2.1', key: '3' },
                    { title: 'Node 2.2', key: '4' },
                ],
            },
        ],
    });
});
*/

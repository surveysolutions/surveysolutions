import Vue from 'vue';

import Vuei18n from './locale'
import { browserLanguage } from './helpers'
const i18n = Vuei18n.initialize(browserLanguage)

import config from './config'
Vue.use(config)

import categories from './components/categories';
import vuetify from './vuetify';

var app = new Vue({
    vuetify : vuetify,
    el: '#app',
    components: {
        categories
    }
});

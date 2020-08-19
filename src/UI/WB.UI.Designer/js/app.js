import Vue from 'vue';
import Vuei18n from './locale'
import config from './config'
import categories from './components/categories';
import vuetify from './vuetify';

Vue.use(config)
//plugin should be loaded and initialized
//before app is created

Vue.use(Vuei18n, (i18next) => {
    new Vue({
        vuetify : vuetify,
        el: '#app',     
        components: {
            categories}
   });

});

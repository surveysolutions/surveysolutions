import Vue from 'vue'
import VueResource from "vue-resource"
import startup from "startup"
import App from "interviewer/InterviewsApp"

Vue.use(VueResource)
Vue.http.headers.common['Authorization'] = input.settings.acsrf.token;

startup(App)
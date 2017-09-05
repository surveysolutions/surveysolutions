import Vue from "vue"

require("bootstrap-sass/assets/javascripts/bootstrap/collapse");

import Breadcrumbs from "./Breadcrumbs"
import Cover from "./Cover"
import Layout from "./Layout"
import Navbar from "./Navbar"
import Section from "./Section"
import Sidebar from "./Sidebar"

Vue.component("Layout", Layout)
Vue.component("Navbar", Navbar)
Vue.component("Sidebar", Sidebar)
Vue.component("Breadcrumbs", Breadcrumbs)
Vue.component("Cover", Cover)
Vue.component("Section", Section)

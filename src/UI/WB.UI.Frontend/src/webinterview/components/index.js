//import Vue from 'vue'

import Breadcrumbs from './Breadcrumbs'
import Cover from './Cover'
import Layout from './Layout'
import Navbar from './Navbar'
import Section from './Section'
import Sidebar from './Sidebar'

export function registerComponents(vue) {

    vue.component('Layout', Layout)
    vue.component('Navbar', Navbar)
    vue.component('Sidebar', Sidebar)
    vue.component('Breadcrumbs', Breadcrumbs)
    vue.component('Cover', Cover)
    vue.component('Section', Section)
}
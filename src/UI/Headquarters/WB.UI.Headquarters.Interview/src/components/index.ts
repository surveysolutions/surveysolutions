import * as vue from "vue"

import Breadcrumbs from "./Breadcrumbs"
import Layout from "./Layout"
import Navbar from "./Navbar"
import Sidebar from "./Sidebar"

vue.component("Layout", Layout)
vue.component("Navbar", Navbar)
vue.component("Sidebar", Sidebar)
vue.component("Breadcrumbs", Breadcrumbs)

vue.component("idle-timeout", (resolve, reject) => {
     require.ensure(["./IdleTimeout"], r => {
        resolve(require("./IdleTimeout").default)
    }, "libs")
})

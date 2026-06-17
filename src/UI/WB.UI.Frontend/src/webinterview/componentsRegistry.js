import { registerComponents } from './components'
import { registerPartsComponents } from './components/questions/parts'
import { registerDerictives } from './directives'

import './misc/htmlPoly.js'

import Idle from './IdleTimeout'

export function registerBaseGlobalComponents(vue, { router, store }) {
    registerComponents(vue)
    registerPartsComponents(vue)
    registerDerictives(vue, { router, store })

    vue.component('IdleTimeoutService', Idle)
}
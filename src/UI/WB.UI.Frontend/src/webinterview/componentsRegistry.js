import { registerComponents } from './components'
import { registerQuestionComponents } from './components/questions'
import { registerPartsComponents } from './components/questions/parts'
import { registerDerictives } from './directives'

import 'jquery-mask-plugin'
import './misc/htmlPoly.js'

import Idle from './IdleTimeout'

export function registerGlobalComponents(vue) {

    registerComponents(vue)
    registerQuestionComponents(vue)
    registerPartsComponents(vue)
    registerDerictives(vue)

    vue.component('IdleTimeoutService', Idle)
}
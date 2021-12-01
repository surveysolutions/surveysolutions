import Vue from 'vue'

Vue.$t = function(str) {
    return str
}
Vue.prototype.$t = function(str) {
    return str
}

//import '~/webinterview/componentsRegistry'
import '~/webinterview/components'
import '~/webinterview/components/questions'
import '~/webinterview/components/questions/parts'
//import '~/webinterview/directives'

import '../parts'
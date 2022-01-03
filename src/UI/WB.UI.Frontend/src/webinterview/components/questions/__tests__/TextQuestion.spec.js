import Vue from 'vue'

Vue.$t = function(str) {
    return str
}
Vue.prototype.$t = function(str) {
    return str
}

import '~/webinterview/components'
import '~/webinterview/components/questions'
import '~/webinterview/components/questions/parts'
import '../parts'
import { mount, createLocalVue  } from '@vue/test-utils'
import TextQuestion from '../TextQuestion.vue'
import Question from '../Question.vue'
import RemoveAnswer from '../parts/RemoveAnswer.vue'
import Vuex from 'vuex'

const localVue = createLocalVue()
localVue.use(Vuex)


describe('TextQuestion component', () => {

    const webinterview = {}
    const entityDetails = {}
    entityDetails['111'] = {
        isLoading : false,
        isDisabled: false,
        isAnswered: true,
        validity: { isValid: true },
        hideIfDisabled: false,
        id: '111',
        answer: 'same test',
    }
    webinterview.fetch = { state: {}}
    webinterview.entityDetails = entityDetails

    let state = {
        webinterview: webinterview,
        route: { hash: '#same' },
    }
    let getters = {
        isReviewMode: () => false,
    }

    let store = new Vuex.Store({
        state,
        getters,
    })

    const wrapper = mount(TextQuestion, {
        propsData: {
            id: '111',
        },
        store,
        localVue,
    })

    it('is visible', () => {
        expect(wrapper.isVisible()).toBe(true)
    })

    it('contains base question behaviour', () => {
        var comp = wrapper.findComponent(Question)
        expect(comp.exists()).toBe(true)
        expect(comp.isVisible()).toBe(true)
    })

    it('contains text editor', () => {
        expect(wrapper.find('textarea-autosize').exists()).toBe(true)
        const html = wrapper.html()
        expect(html.includes('textarea')).toBe(true)
    })

    it('contains remove answer button', () => {
        expect(wrapper.findComponent(RemoveAnswer).exists()).toBe(true)
    })
})

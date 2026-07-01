import '~/webinterview/components'
import '~/webinterview/components/questions'
import '~/webinterview/components/questions/parts'
import '../parts'
import '~/webinterview/directives/DateTimeFormatting'
import '~/webinterview/directives/MaskedText'
import '~/webinterview/directives/LinkToRoute'
import { mount } from '@vue/test-utils'
import TextQuestion from '../TextQuestion.vue'
import Question from '../Question.vue'
import RemoveAnswer from '../parts/RemoveAnswer.vue'
import { createStore } from 'vuex'


describe('TextQuestion component', () => {

    const webinterview = {}
    const entityDetails = {}
    entityDetails['111'] = {
        isLoading: false,
        isDisabled: false,
        isAnswered: true,
        validity: { isValid: true, warnings: [] },
        hideIfDisabled: false,
        id: '111',
        answer: 'same test',
        comments: [],
    }
    webinterview.fetch = { state: {} }
    webinterview.entityDetails = entityDetails

    let state = {
        webinterview: webinterview,
        route: { hash: '#same' },
    }
    let getters = {
        isReviewMode: () => false,
    }

    const store = createStore({
        state,
        getters,
    })

    const wrapper = mount(TextQuestion, {
        props: {
            id: '111',
        },
        global: {
            plugins: [store],
            mocks: {
                $t: (str) => str,
            },
            components: {
                'wb-question': Question,
                'wb-remove-answer': RemoveAnswer,
                'wb-lock': { template: '<div />' },
                'textarea-autosize': { template: '<textarea />' },
            },
            directives: {
                blurOnEnterKey: () => { },
                maskedText: () => { },
                autosize: () => { },
            },
        },
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
        expect(wrapper.find('textarea').exists()).toBe(true)
        const html = wrapper.html()
        expect(html.includes('textarea')).toBe(true)
    })

    it('contains remove answer button', () => {
        expect(wrapper.findComponent(RemoveAnswer).exists()).toBe(true)
    })
})

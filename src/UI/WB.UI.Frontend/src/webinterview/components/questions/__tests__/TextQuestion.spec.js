import BaseQuestion from './BaseQuestion'
import { shallowMount, mount } from '@vue/test-utils'
import Vue from 'vue'
import TextQuestion from '../TextQuestion.vue'


describe('TextQuestion component', () => {


    const wrapper = mount(TextQuestion, {
        propsData: {
            //id: '1111',
        },
    })

    wrapper.$store = {}
    wrapper.$store.state = {}
    wrapper.$store.state.webinterview = {}
    wrapper.$store.state.webinterview.entityDetails = {}
    wrapper.$store.state.webinterview.entityDetails['1111'] = { isLoading : false }
    wrapper.setData({id: '1111'})

    var questionHtml = wrapper.html()


    it('contains text editor', () => {
        expect(questionHtml).toContain('<wb-question')
    })

    it('contains text editor', () => {
        expect(questionHtml).toContain('<textarea-autosize')
    })

    it('contains remove answer button', () => {
        expect(questionHtml).toContain('<wb-remove-answer')
    })
})

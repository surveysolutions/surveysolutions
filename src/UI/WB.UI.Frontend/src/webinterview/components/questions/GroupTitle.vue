<template>
    <wb-question :question="$me" :questionDivCssClassName="titleClass" :questionCssClassName="statusClass"
        noTitle="true" noValidation="true" noInstructions="true" noComments="true" noFlag="true">
        <span v-dompurify-html="$me.title"></span><span v-if="this.$me.isRoster && !this.$me.hasCustomRosterTitle"> -
            <i>{{ rosterTitle }}</i></span>
    </wb-question>
</template>

<script lang="js">
import { entityDetails } from '../mixins'
import { GroupStatus } from './index'
import { debounce } from 'lodash'

export default {
    name: 'GroupTitle',
    mixins: [entityDetails],

    watch: {
        ['$store.getters.scrollState']() {
            this.scroll()
        },
    },

    mounted() {
        this.scroll()
    },

    computed: {
        navigateTo() {
            return {
                name: 'section', params: {
                    sectionId: this.id,
                    interviewId: this.$route.params.interviewId,
                },
            }
        },
        rosterTitle() {
            return this.$me.rosterTitle ? `${this.$me.rosterTitle}` : '[...]'
        },
        isNotStarted() {
            return this.$me.status === GroupStatus.NotStarted
        },
        isStarted() {
            return this.$me.status === GroupStatus.Started
        },
        isCompleted() {
            return this.$me.status === GroupStatus.Completed
        },
        titleClass() {
            return ['roster-title']
        },
        statusClass() {
            return ['roster-section-block', {
                'started': this.$me.validity.isValid && this.isStarted,
                'has-error': !this.$me.validity.isValid,
                '': this.$me.validity.isValid && !this.isCompleted,
            },
                {
                    'answered': this.isCompleted,
                }]
        },
    },
    methods: {
        doScroll: debounce(function () {
            if (this.$store.getters.scrollState == this.id) {
                window.scroll({ top: this.$el.offsetTop, behavior: 'smooth' })
                this.$store.dispatch('resetScroll')
            }
        }, 200),

        scroll() {
            if (this.$store && this.$store.state.route.hash === '#' + this.id) {
                this.doScroll()
            }
        },
    },
}
</script>

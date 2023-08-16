<template>
    <div class="question static-text"
        v-if="!$me.isLoading"
        :id="hash">
        <div class="question-editor">
            <div>
                <h5 v-dateTimeFormatting
                    v-linkToRoute
                    v-html="title"></h5>
            </div>
        </div>
    </div>
</template>
<script lang="js">
import { entityDetails } from '../mixins'
import { getLocationHash } from '~/shared/helpers'
import { debounce } from 'lodash'

export default {
    name: 'Variable',
    mixins: [entityDetails],
    watch: {
        ['$store.getters.scrollState']() {
            this.scroll()
        },
    },
    data() {
        return {
            text: '',
        }
    },

    mounted() {
        this.scroll()
    },
    computed: {
        hash() {
            return getLocationHash(this.$me.id)
        },
        title() {
            let value = this.$me.value && this.$me.value != ''
                ? this.$me.value
                : this.$t('WebInterviewUI.NotCalculated')
            return (this.$me.title || this.$me.name) + ' - ' + value
        },
    },
    methods : {
        doScroll: debounce(function() {
            if(this.$store.getters.scrollState == '#' + this.id){
                window.scroll({ top: this.$el.offsetTop, behavior: 'smooth' })
                this.$store.dispatch('resetScroll')
            }
        }, 200),

        scroll() {
            if(this.$store && this.$store.state.route.hash === '#' + this.id) {
                this.doScroll()
            }
        },
    },
}
</script>

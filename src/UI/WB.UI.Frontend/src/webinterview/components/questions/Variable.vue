<template>
    <div class="question static-text variable" :class="variableClass" v-if="!$me.isLoading" :id="hash">
        <div class="question-editor">
            <div>
                <h5>
                    <a class="open-designer" v-if="this.$config.inWebTesterMode && $me.name" href="javascript:void(0);"
                        @click="openDesigner" v-dompurify-html="'[' + $me.name + ']'"></a>
                    <span v-dateTimeFormatting v-linkToRoute v-dompurify-html="title"></span>
                </h5>
            </div>
        </div>
    </div>
</template>
<script lang="js">
import { entityDetails } from '../mixins'
import { getLocationHash } from '~/shared/helpers'
import { debounce, find } from 'lodash'

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
        variableClass() {
            const entity = find(this.$store.state.webinterview.entities, d => d.identity == this.id)
            return [
                {
                    'section-variable': !entity.isCover
                },
            ]
        },
    },
    methods: {
        doScroll: debounce(function () {
            if (this.$store.getters.scrollState == '#' + this.id) {
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

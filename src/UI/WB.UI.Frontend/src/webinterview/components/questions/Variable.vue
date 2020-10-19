<template>
    <div class="question static-text"
        v-if="!$me.isLoading"
        :id="hash">
        <div class="question-editor">
            <div>
                <wb-title />
                <div class="question-unit">
                    <div class="options-group">
                        <div class="form-group">
                            <div class="field"
                                :class="{answered: $me.value}">
                                {{ $me.value }}
                            </div>
                        </div>
                    </div>
                </div>
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

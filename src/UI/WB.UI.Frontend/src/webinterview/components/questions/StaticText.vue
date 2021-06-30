<template>
    <div class="question static-text"
        v-if="!$me.isLoading && !($me.isDisabled && $me.hideIfDisabled)"
        :class="[{'disabled-question': $me.isDisabled}]"
        :id="hash">
        <div class="question-editor">
            <div>
                <wb-title />
            </div>
            <wb-attachment :contentId="$me.attachmentContent"
                :interviewId="interviewId"
                customCssClass="static-text-image"
                v-if="$me.attachmentContent && !$me.isDisabled" />
            <wb-validation />
            <wb-warnings />
        </div>
    </div>
</template>
<script lang="js">
import { entityDetails } from '../mixins'
import { getLocationHash } from '~/shared/helpers'
import { debounce } from 'lodash'

export default {
    name: 'StaticText',
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

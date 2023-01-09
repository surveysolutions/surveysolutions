<template>
    <div class="progress question-progress"
        :class="{'complited' : progress == 100}"
        v-if="isVisible">
        <div class="progress-bar active"
            :class="{'progress-bar-striped':striped}"
            role="progressbar"
            :style="style">
            <span class="sr-only"> {{ $t("WebInterviewUI.ProgressText", { progress } ) }}</span>
        </div>
    </div>
</template>
<script lang="js">
import { delay } from 'lodash'

export default {
    name: 'wb-progress',
    props: {
        valuenow: { type: Number, default: 100 },
        valuemax: { type: Number, default: 100 },
        visible: { type: Boolean, default: false },
        delay: { type: Number, default: 150 },
        striped: { type: Boolean, default: true },
    },
    watch: {
        visible(to, from) {
            if (from === false) {
                this.timerId = delay(() => this.isVisible = to, this.delay)
            } else {
                if(this.timerId != null){
                    clearTimeout(this.timerId)
                    this.timerId = null
                }

                this.isVisible = this.to
            }
        },
    },
    data() {
        return {
            isVisible: false,
            timerId: null,
        }
    },
    created() {
        this.isVisible = this.visible
    },
    computed: {
        progress() {
            return Math.round((this.valuenow / this.valuemax) * 100)
        },

        style() {
            return {
                width: this.progress + '%',
            }
        },
    },
}
</script>

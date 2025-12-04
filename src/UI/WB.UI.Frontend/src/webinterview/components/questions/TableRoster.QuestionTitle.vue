<template>
    <popover class="w-100 d-block" :enable="hasInstructions" trigger="hover-focus" append-to="body">
        <div class="ag-cell-label-container" v-bind:class="{ 'has-instruction': hasInstructions }">
            <div class="ag-header-cell-label">

                <a class="open-designer" style="word-break: break-all;" v-if="this.$config.inWebTesterMode && name"
                    href="javascript:void(0);" @click="openDesigner(questionId)"
                    v-dompurify-html="'[' + name + ']'"></a>
                <span class="ag-header-cell-text" style="word-break: break-all;" v-dompurify-html="title"></span>
            </div>
        </div>
        <template v-slot:popover>
            <div class="instruction-tooltip">
                <span v-dateTimeFormatting v-dompurify-html="instruction"></span>
            </div>
        </template>
    </popover>
</template>

<script lang="js">
import { find } from 'lodash'
import { designerMixin } from '../mixins'
export default {
    name: 'TableRoster_QuestionTitle',
    mixins: [designerMixin],

    data() {
        return {
            title: null,
            instruction: null,
            hasInstructions: false,
            questionId: null,
            name: null,
        }
    },
    computed: {

    },
    methods: {

    },
    created() {
        this.questionId = this.params.questionId
        this.title = this.params.title
        this.instruction = this.params.instruction
        this.hasInstructions = this.instruction != undefined && this.instruction != null && this.instruction != ''
        this.name = this.params.name
    },
    watch: {
        ['params.context.componentParent.$me.questions']() {
            var self = this
            var question = find(self.params.context.componentParent.$me.questions, function (o) { return o.id == self.questionId })
            if (question !== undefined) {
                this.instruction = question.instruction
                this.name = question.name
            }
        },
        ['$config.inWebTesterMode']() {
            var self = this
            var question = find(self.params.context.componentParent.$me.questions, function (o) { return o.id == self.questionId })
            if (question !== undefined) {
                this.name = question.name
            }
        },
    },
}
</script>

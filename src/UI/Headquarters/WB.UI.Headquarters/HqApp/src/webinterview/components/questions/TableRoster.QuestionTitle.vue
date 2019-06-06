<template>
    <popover :enable="hasInstructions" trigger="hover-focus" append-to="body">
        <div class="ag-cell-label-container">    
            <div class="ag-header-cell-label">    
                <span class="ag-header-cell-text" v-html="title"></span>    
            </div>
        </div>
        <template slot="popover">
            <div class="popover-content information-block instruction">        
                <span v-dateTimeFormatting v-html="instruction"></span>
            </div>
        </template>        
    </popover>
</template>

<script lang="js">
    export default {
        name: 'TableRoster_QuestionTitle',

        data() {
            return {
                title: null,
                instruction: null,
                hasInstructions: false,
            }
        }, 
        computed: {

        },
        methods: {

        },
        created() {
            this.title = this.params.title
            this.instruction = this.params.instruction
            this.hasInstructions = this.instruction != undefined && this.instruction != null && this.instruction != ''
        }
    }
</script>












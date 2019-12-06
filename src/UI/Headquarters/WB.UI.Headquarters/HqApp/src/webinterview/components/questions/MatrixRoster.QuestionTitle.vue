<template>
    <popover class="w-100 d-block" :enable="hasInstructions" trigger="hover-focus" append-to="body"><!-- v-bind:style="{ width: getWidth() + 'px!important' }"-->
        <div class="ag-cell-label-container" v-bind:class="{ 'has-instruction' : hasInstructions }">    
            <div class="ag-header-cell-label" style="padding:0px;">
                <div v-if="question.entityType =='CategoricalSingle' || question.entityType =='CategoricalMulti'" 
                class="ag-header-cell-text" style="display:flex;">
                    <div v-for="option in getOptions" :key="question.id + '_' + option.value">
                        <div style="width:220px;border: 1px solid #E5E5E5; margin:3px;">{{option.title}}</div>                         
                    </div>
                </div> 
                 
            </div>
        </div>
        <!--template slot="popover">
            <div class="instruction-tooltip">        
                <span v-dateTimeFormatting v-html="instruction"></span>
            </div>
        </template-->        
    </popover>
</template>

<script lang="js">
        
    export default {
        name: 'MatrixRoster_QuestionTitle',

        data() {
            return {
                title: null,
                instruction: null,
                hasInstructions: false,
                question: null
            }
        }, 
        computed: {    
            //$me() { return this.$store.state.webinterview.entityDetails[this.question.identity] },
            getOptions(){                
                return this.question.options;                
            }
        },
        methods: {
            getWidth(){
                return this.getOptions.length * 226; 
            }
        },
        created() {
            this.title = this.params.title
            this.instruction = this.params.instruction
            //this.hasInstructions = this.instruction != undefined && this.instruction != null && this.instruction != ''
            this.question = this.params.question
        }
    }
</script>












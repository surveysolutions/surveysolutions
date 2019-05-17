<template>
    <div class="question table-view scroller" :id="hash">
        <ag-grid-vue 
            class="ag-theme-customStyles"
            :defaultColDef="defaultColDef"
            :columnDefs="columnDefs"
            :rowData="rowData"

            rowHeight="40"
            domLayout='autoHeight'
            headerHeight="50"

            @grid-ready="onGridReady"
            @onCellEditingStopped="endCellEditting">
        </ag-grid-vue>
    </div>
</template>

<script lang="js">
    import Vue from 'vue'
    import { entityDetails } from "../mixins"
    import { GroupStatus } from "./index"
    import { debounce, every, some } from "lodash"
    import { AgGridVue } from "ag-grid-vue";

    import TableRoster_QuestionEditor from "./TableRoster.QuestionEditor";
    import TableRoster_ViewAnswer from "./TableRoster.ViewAnswer";
    
    export default {
        name: 'TableRoster',
        mixins: [entityDetails],
        
        data() {
            return {
                defaultColDef: null,
                questionEditors: null,
                columnDefs: null,
                rowData: null,
                gridApi: null,
                columnApi: null,
            }
        },

        components: {
            AgGridVue,
            TableRoster_ViewAnswer,
            TableRoster_QuestionEditor,
        },

        beforeMount() {

            this.defaultColDef = {
                width: 220, // set every column width
                height: 76,
                resizable: true,
                editable: true // make every column editable
            };

            this.initQuestionAsColumns()
            this.initQuestionsInRows()
        },

        watch: {
            ["$store.getters.scrollState"]() {
                 this.scroll();
            },
            ["$me.instances"]() {
                this.initQuestionsInRows()
            }
        },

        mounted() {
            this.scroll();
        },

        computed: {
                      
        },
        methods : {
            initQuestionAsColumns() {
                var self = this;
                var columnsFromQuestions = _.map(
                    this.$me.questions,
                    (question, key) => {
                        return {
                            headerName: question.title, 
                            field: question.id, 
                            cellRendererFramework: 'TableRoster_ViewAnswer',
                            cellRendererParams: {
                                id: question.id,
                            },
                            cellEditorFramework: 'TableRoster_QuestionEditor', 
                            cellEditorParams: {
                                id: question.id,
                                value: question,
                            },
                        };
                    }
                );
                columnsFromQuestions.unshift({headerName: "", field: "rosterTitle", pinned: true, editable: false});
                this.columnDefs = columnsFromQuestions
            },

            initQuestionsInRows() {
                var self = this;

                var rosterInstancesWithQuestionsAsRows = _.map(
                    this.$me.instances,
                    (instance, key) => {
                        var instanceAsRow = {
                            rosterVector: instance.rosterVector,
                            rosterTitle: instance.rosterTitle, 
                        };
                        self.$me.questions.forEach(question => {
                            var questionIdentity = question.id + instance.rosterVector
                            instanceAsRow[question.id] = {
                                identity : questionIdentity,
                                type     : question.entityType
                            }
                        });
                        
                        return instanceAsRow;
                    }
                )
                this.rowData = rosterInstancesWithQuestionsAsRows
            },

            onGridReady(params) {
                //params.api.sizeColumnsToFit(220);
                //var $this = $(this);
                //var height = $this.find('.ag-cell-label-container').height();
                //params.api.setHeaderHeight(height);
                //setHeaderHeight

                this.gridApi = params.api;
                this.columnApi = params.columnApi;
            },

            doScroll: debounce(function() {
                if(this.$store.getters.scrollState == this.id){
                    window.scroll({ top: this.$el.offsetTop, behavior: "smooth" })
                    this.$store.dispatch("resetScroll")
                }
            }, 200),

            scroll() {
                if(this.$store && this.$store.state.route.hash === "#" + this.id) {
                    this.doScroll(); 
                }
            },

            endCellEditting(event) {
			    console.log('cellEditingStopped, value:' + event.value);
		    }
        }
    }
</script>

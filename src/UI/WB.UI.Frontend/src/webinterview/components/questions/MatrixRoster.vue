<template>
    <div class="question table-view scroller"
        :id="hash"
        v-if="rowData.length > 0">
        <div class="question-editor">
            <h5 v-dateTimeFormatting
                v-html="title"></h5>
            <div class="information-block instruction"
                v-if="instructions">
                <p v-dateTimeFormatting
                    v-html="instructions"></p>
            </div>
        </div>
        <ag-grid-vue
            ref="matrixRoster"
            class="ag-theme-customStyles roster-matrix"
            domLayout="autoHeight"
            rowHeight="40"
            headerHeight="50"
            :defaultColDef="defaultColDef"
            :columnDefs="columnDefs"
            :rowData="rowData"
            :grid-options="gridOptions"
            @grid-ready="onGridReady"
            @column-resized="autosizeHeaders"></ag-grid-vue>
    </div>
</template>

<script lang="js">
/* eslint-disable vue/no-unused-components */
import Vue from 'vue'
import { entityDetails } from '../mixins'
import { GroupStatus } from './index'
import { debounce, every, some, map } from 'lodash'
import { AgGridVue } from 'ag-grid-vue'

import MatrixRoster_QuestionEditor from './MatrixRoster.QuestionEditor'
import MatrixRoster_RosterTitle from './MatrixRoster.RosterTitle'
import MatrixRoster_QuestionTitle from './MatrixRoster.QuestionTitle'
import MatrixRoster_CategoricalSingle from './MatrixRoster.CategoricalSingle'

export default {
    name: 'MatrixRoster',
    mixins: [entityDetails],

    data() {
        return {
            defaultColDef: null,
            questionEditors: null,
            columnDefs: null,
            rowData: null,
            gridApi: null,
            columnApi: null,
            countOfInstances: 0,
            title: null,
            instructions: null,
        }
    },

    components: {
        AgGridVue,
        MatrixRoster_QuestionEditor,
        MatrixRoster_RosterTitle,
        MatrixRoster_QuestionTitle,
        MatrixRoster_CategoricalSingle,
    },

    beforeMount() {
        this.countOfInstances = this.$me.instances.length
        this.title = this.$me.questions.length > 0 ? this.$me.questions[0].title : null
        this.instructions = this.$me.questions.length > 0 ? this.$me.questions[0].instruction : null

        this.defaultColDef = {
            width: 180, // set every column width
            height: 76,
            resizable: true,
            editable: false, // make every column editable
            autoHeight: true,
            wrapText: true,
        }

        this.initQuestionAsColumns()
        this.initQuestionsInRows()
    },

    watch: {
        ['$store.getters.scrollState']() {
            this.scroll()
        },
        ['$me.instances']() {
            if (this.countOfInstances != this.$me.instances.length) {
                this.countOfInstances = this.$me.instances.length
                this.initQuestionsInRows()
                this.setTableRosterHeight()
            }
        },
        ['$me.questions']() {
            this.instructions = this.$me.questions.length > 0 ? this.$me.questions[0].instruction : null
        },
        ['$me.title']() {
            this.title = this.$me.title
        },
    },

    mounted() {
        this.scroll()
    },

    computed: {
        gridOptions() {
            return {
                suppressClickEdit:true,
                suppressCellSelection:true,
                suppressMovableColumns:true,
                context: {
                    componentParent: this,
                },
            }
        },
    },
    methods : {
        initQuestionAsColumns() {
            var self = this
            var columnsFromQuestions = map(
                this.$me.questions,
                (question, key) => {
                    return {
                        headerName: question.title,
                        headerComponentFramework: 'MatrixRoster_QuestionTitle',
                        headerComponentParams: {
                            //title: question.title,
                            instruction: question.instruction,
                            question: question,
                            attachmentImageLoadedCallback: () => self.attachmentImageLoaded(),
                        },
                        width:question.options.length * 180,

                        field: question.id,
                        cellRendererFramework: 'MatrixRoster_QuestionEditor',
                        cellRendererParams: {
                            id: question.id,
                            question: question,
                            value: question,
                        },
                        //cellEditorFramework: 'MatrixRoster_QuestionEditor',
                        //cellEditorParams: {
                        //    id: question.id,
                        //    value: question,
                        //},
                    }
                }
            )
            columnsFromQuestions.unshift({
                headerName: '',//this.$me.title,
                field: 'rosterTitle',
                autoHeight: true,
                pinned: true,
                editable: false,
                cellStyle: {minHeight: '40px'},
                cellRendererFramework: 'MatrixRoster_RosterTitle',
                cellRendererParams: { },
            })
            this.columnDefs = columnsFromQuestions
        },

        initQuestionsInRows() {
            var self = this

            var rosterInstancesWithQuestionsAsRows = map(
                this.$me.instances,
                (instance, key) => {
                    var instanceAsRow = {
                        rosterVector: instance.rosterVector,
                        rosterTitle: {
                            matrixRoster: self,
                            rowIndex: key,
                        },
                    }
                    self.$me.questions.forEach(question => {
                        var questionIdentity = question.id + instance.rosterVector
                        instanceAsRow[question.id] = {
                            identity : questionIdentity,
                            type     : question.entityType,
                        }
                    })

                    return instanceAsRow
                }
            )
            this.rowData = rosterInstancesWithQuestionsAsRows
        },

        onGridReady(params) {
            this.gridApi = params.api
            this.columnApi = params.columnApi

            this.autosizeHeaders(params)
            this.setTableRosterHeight()
        },

        autosizeHeaders(event) {
            if (this.gridApi && event && (event === true || event.finished !== false)) {
                const self = this
                const MIN_HEIGHT = 16
                this.gridApi.setHeaderHeight(MIN_HEIGHT)
                const headerCells = $(this.$refs.matrixRoster.$el).find('.ag-header-cell-label')
                let minHeight = MIN_HEIGHT
                for (let index = 0; index < headerCells.length; index++) {
                    const cell = headerCells[index]
                    minHeight = Math.max(minHeight, cell.scrollHeight)
                }

                // set header height to calculated height + padding (top: 8px, bottom: 8px)
                this.gridApi.setHeaderHeight(minHeight)

                // set all rows height to auto
                this.gridApi.resetRowHeights()
            }
        },

        setTableRosterHeight() {
            if(this.$refs.tableRoster != undefined)
            {
                if (this.$me.instances.length > 30) {
                    this.gridApi.setDomLayout('normal')
                    this.$refs.matrixRoster.$el.style.height = '1536px'
                }
                else {
                    this.gridApi.setDomLayout('autoHeight')
                    this.$refs.matrixRoster.$el.style.height = ''
                }
            }
        },

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

        attachmentImageLoaded() {
            this.autosizeHeaders(true)
            this.setTableRosterHeight()
        },
    },
}
</script>

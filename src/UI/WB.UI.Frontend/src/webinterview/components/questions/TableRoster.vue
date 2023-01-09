<template>
    <div class="question table-view scroller"
        :id="hash"
        v-if="rowData.length > 0">
        <ag-grid-vue
            ref="tableRoster"
            class="ag-theme-customStyles"
            domLayout="autoHeight"
            rowHeight="40"
            headerHeight="50"
            :defaultColDef="defaultColDef"
            :columnDefs="columnDefs"
            :rowData="rowData"
            :grid-options="gridOptions"
            @grid-ready="onGridReady"
            @column-resized="autosizeHeaders"
            @cell-editing-stopped="endCellEditting"></ag-grid-vue>
    </div>
</template>

<script lang="js">
/* eslint-disable vue/no-unused-components */

import Vue from 'vue'
import { entityDetails } from '../mixins'
import { GroupStatus } from './index'
import { debounce, every, some, map } from 'lodash'
import { AgGridVue } from 'ag-grid-vue'

import TableRoster_QuestionEditor from './TableRoster.QuestionEditor'
import TableRoster_ViewAnswer from './TableRoster.ViewAnswer'
import TableRoster_RosterTitle from './TableRoster.RosterTitle'
import TableRoster_QuestionTitle from './TableRoster.QuestionTitle'
import TableRoster_Title from './TableRoster.Title'

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
            countOfInstances: 0,
        }
    },

    components: {
        AgGridVue,
        TableRoster_ViewAnswer,
        TableRoster_QuestionEditor,
        TableRoster_RosterTitle,
        TableRoster_QuestionTitle,
        TableRoster_Title,
    },

    beforeMount() {
        this.countOfInstances = this.$me.instances.length

        this.defaultColDef = {
            width: 180, // set every column width
            height: 76,
            resizable: true,
            editable: true, // make every column editable
            autoHeight: true,
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
    },

    mounted() {
        this.scroll()
    },

    computed: {
        gridOptions() {
            return {
                stopEditingWhenGridLosesFocus: true,
                suppressMovableColumns:true,
                singleClickEdit:true,
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
                        headerComponentFramework: 'TableRoster_QuestionTitle',
                        headerComponentParams: {
                            title: question.title,
                            instruction: question.instruction,
                            questionId: question.id,
                        },
                        field: question.id,
                        cellRendererFramework: 'TableRoster_ViewAnswer',
                        cellRendererParams: {
                            id: question.id,
                            question: question,
                        },
                        cellEditorFramework: 'TableRoster_QuestionEditor',
                        cellEditorParams: {
                            id: question.id,
                            value: question,
                        },
                    }
                }
            )
            columnsFromQuestions.unshift({
                headerName: this.$me.title,
                headerComponentFramework: 'TableRoster_Title',
                headerComponentParams: {
                    title: this.$me.title,
                },
                field: 'rosterTitle',
                autoHeight: true,
                pinned: true,
                editable: false,
                cellStyle: {minHeight: '40px'},
                cellRendererFramework: 'TableRoster_RosterTitle',
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
                            tableRoster: self,
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
            if (event.finished !== false) {
                const MIN_HEIGHT = 16
                event.api.setHeaderHeight(MIN_HEIGHT)
                const headerCells = this.$refs.tableRoster.$el.getElementsByClassName('ag-header-cell-label')
                let minHeight = MIN_HEIGHT
                for (let index = 0; index < headerCells.length; index++) {
                    const cell = headerCells[index]
                    minHeight = Math.max(minHeight, cell.scrollHeight)
                }

                // set header height to calculated height + padding (top: 8px, bottom: 8px)
                event.api.setHeaderHeight(minHeight)

                // set all rows height to auto
                event.api.resetRowHeights()
            }
        },

        setTableRosterHeight() {
            if(this.$refs.tableRoster != undefined)
            {
                if (this.$me.instances.length > 20) {
                    this.gridApi.setDomLayout('normal')
                    this.$refs.tableRoster.$el.style.height = '1024px'
                }
                else {
                    this.gridApi.setDomLayout('autoHeight')
                    this.$refs.tableRoster.$el.style.height = ''
                }
            }
        },

        doScroll: debounce(function() {
            if(this.$store.getters.scrollState == this.id){
                window.scroll({ top: this.$el.offsetTop, behavior: 'smooth' })
                this.$store.dispatch('resetScroll')
            }
        }, 200),

        scroll() {
            if(this.$store && this.$store.state.route.hash === '#' + this.id) {
                this.doScroll()
            }
        },

        endCellEditting(event) {
            event.api.resetRowHeights()
        },
    },
}
</script>

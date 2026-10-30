<template>
    <div class="question table-view scroller"
        :id="hash"
        v-if="rowData.length > 0">
        <ag-grid-vue ref="tableRoster"
            class="ag-theme-customStyles roster-table"
            domLayout="autoHeight"
            :rowHeight="40"
            :headerHeight="50"
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

import { entityDetails } from '../mixins'
import { debounce, every, some, map } from 'lodash-es'
import { AgGridVue, agGridTheme } from './agGrid'

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
            //height: 76,
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
                theme: agGridTheme,
                stopEditingWhenCellsLoseFocus: true,
                suppressMovableColumns: true,
                singleClickEdit: true,
                context: {
                    componentParent: this,
                },
            }
        },
    },
    methods: {
        initQuestionAsColumns() {
            var self = this
            var columnsFromQuestions = map(
                this.$me.questions,
                (question, key) => {
                    return {
                        headerName: question.title,
                        headerComponent: 'TableRoster_QuestionTitle',
                        headerComponentParams: {
                            title: question.title,
                            instruction: question.instruction,
                            questionId: question.id,
                            name: question.name,
                        },
                        field: question.id,
                        cellRenderer: 'TableRoster_ViewAnswer',
                        cellRendererParams: {
                            id: question.id,
                            question: question,
                        },
                        cellEditor: 'TableRoster_QuestionEditor',
                        cellEditorParams: {
                            id: question.id,
                            value: question,
                        },
                        valueFormatter: () => '',
                        cellDataType: false,
                    }
                }
            )
            columnsFromQuestions.unshift({
                headerName: this.$me.title,
                headerComponent: 'TableRoster_Title',
                headerComponentParams: {
                    title: this.$me.title,
                },
                field: 'rosterTitle',
                autoHeight: true,
                pinned: true,
                editable: false,
                cellStyle: { minHeight: '40px' },
                cellRenderer: 'TableRoster_RosterTitle',
                cellRendererParams: {},
                valueFormatter: () => '',
                cellDataType: false,
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
                    self.$me.questions.forEach((question) => {
                        var questionIdentity =
                            question.id + instance.rosterVector
                        instanceAsRow[question.id] = {
                            identity: questionIdentity,
                            type: question.entityType,
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
                event.api.setGridOption('headerHeight', MIN_HEIGHT)
                const headerCells =
                    this.$refs.tableRoster.$el.getElementsByClassName(
                        'ag-header-cell-label'
                    )
                let minHeight = MIN_HEIGHT
                for (let index = 0; index < headerCells.length; index++) {
                    const cell = headerCells[index]
                    minHeight = Math.max(minHeight, cell.scrollHeight)
                }

                // set header height to calculated height + padding (top: 8px, bottom: 8px)
                event.api.setGridOption('headerHeight', minHeight)
            }
        },

        setTableRosterHeight() {
            if (this.$refs.tableRoster != undefined) {
                if (this.$me.instances.length > 20) {
                    this.gridApi.setGridOption('domLayout', 'normal')
                    this.$refs.tableRoster.$el.style.height = '1024px'
                } else {
                    this.gridApi.setGridOption('domLayout', 'autoHeight')
                    this.$refs.tableRoster.$el.style.height = ''
                }
            }
        },

        doScroll: debounce(function () {
            if (this.$store.getters.scrollState == this.id) {
                const navbarHeight = document.querySelector('.navbar-fixed-top')?.offsetHeight || 0
                window.scroll({ top: this.$el.offsetTop - navbarHeight, behavior: 'smooth' })
                this.$store.dispatch('resetScroll')
            }
        }, 200),

        scroll() {
            if (this.$store && this.$store.state.route.hash === '#' + this.id) {
                this.doScroll()
            }
        },

        endCellEditting(event) {},
    },
}
</script>

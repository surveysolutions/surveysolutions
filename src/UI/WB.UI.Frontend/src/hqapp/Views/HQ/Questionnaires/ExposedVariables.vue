<template>
    <HqLayout :hasFilter="false">
        <template slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a href="/SurveySetup">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
            </ol>
            <h1>{{$t('Pages.Questionnaire_Exposed_Variables')}}</h1>
        </template>

        <div class="row">
            <div class="col-sm-8">
                <h2>
                    {{$t('Pages.QuestionnaireNameFormat', { name : model.title, version : model.version})}}
                </h2>
            </div>
        </div>

        <div class="row">
            <div class="col-sm-6 col-xs-10 info-block">
                <p>
                    {{$t('Pages.Exposed_Variables_Description')}}
                </p>
            </div>
        </div>

        <div class="row">
            <div class="col-sm-6">

                <DataTables
                    id="id-table"
                    ref="table"
                    :tableOptions="tableOptions"
                    @cell-clicked="cellAllClicked"
                    noSelect
                    noSearch
                    :noPaging="false">
                </DataTables>
            </div>
            <div class="col-sm-6">

                <table class="table table-striped table-bordered">
                    <tbody>
                        <tr v-for="variable in exposedVariables"
                            :key="'id' + '__' + variable.id"
                            @click="cellExposedClicked(variable.id)">
                            <td>{{variable.variableName}}</td>
                            <td>{{variable.title}}</td>
                        </tr>
                    </tbody>
                </table>
                <!-- <DataTables
                    id="id-table-exposed"
                    ref="table-exposed"
                    :tableOptions="tableOptionsExposed"
                    noSelect
                    noSearch
                    :noPaging="false">
                </DataTables> -->

                <div class="action-buttons">
                    <button
                        @click="saveVariables"
                        class="btn btn-success">
                        {{$t('Common.Save')}}
                    </button>
                </div>
            </div>
        </div>

        <ModalFrame ref="exposedChangeModal"
            :title="$t('Pages.ConfirmationNeededTitle')"
            :canClose="false">
            <p>{{ $t("Pages.ExposedChange" )}}</p>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-danger"
                    v-bind:disabled="model.isObserving"
                    @click="changeExposedStatusSend">{{ $t("Common.Ok") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>

    </HqLayout>
</template>

<script>
import { keyBy, map, find, filter, escape } from 'lodash'
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'

export default {

    data() {

        return {
            exposedVariables: [],
        }
    },

    computed: {
        model() {
            return this.$config.model
        },
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'variableName',
                        name: 'VariableName',
                        title: 'Variable Name',
                        sortable: false,
                    },
                    {
                        data: 'title',
                        name: 'Title',
                        title: this.$t('Pages.Title'),
                        sortable: false,
                    },
                    // {
                    //     data: 'isExposed',
                    //     name: 'IsExposed',
                    //     title: this.$t('Workspaces.Name'),
                    //     sortable: false,
                    //     'render': function (data, type, row) {
                    //         if (data === true) {
                    //             return '<input type="checkbox" id="chkadd_' + row.id + '" checked value="true">'
                    //         }
                    //         else {
                    //             return '<input type="checkbox" id="chkadd_' + row.id + '" >'
                    //         }
                    //     },
                    // },
                ],
                rowId: function(row) {
                    return row.id
                },
                ajax: {
                    url: this.$config.model.dataUrl + '?id=' + this.$config.model.questionnaireIdentity,
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
    },
    mounted() {
        this.$hq.Questionnaire(this.model.questionnaireId, this.model.version)
            .ExposedVariables(this.$config.model.questionnaireIdentity)
            .then(data => {
                this.exposedVariables = map(data.data, d => {
                    return {
                        id: d.id,
                        title: d.title,
                        variableName : d.variableName,
                    }
                })
            })
    },
    methods: {


        saveVariables(){
            this.$refs.exposedChangeModal.modal({keyboard: false})

        },

        async changeExposedStatusSend() {
            const response = await this.$hq.Questionnaire(this.model.questionnaireId, this.model.version)
                .ChangeVariableExposedStatus( this.exposedVariables)
            this.$refs.exposedChangeModal.modal('hide')
        },

        cellAllClicked(columnName, rowId, cellData) {
            const parsedRowId = rowId.replace('id__', '')

            var rowData = this.$refs.table.table.row('#'+rowId).data()

            var index = this.exposedVariables.findIndex(x => x.id == parsedRowId)
            if(index === -1)
                this.exposedVariables.push({
                    id: parsedRowId,
                    title : rowData.title,
                    variableName : rowData.variableName,
                })
        },
        cellExposedClicked(id)
        {
            var index = this.exposedVariables.findIndex(x => x.id == id)
            this.exposedVariables.splice(index,1)
        },

    },
}
</script>

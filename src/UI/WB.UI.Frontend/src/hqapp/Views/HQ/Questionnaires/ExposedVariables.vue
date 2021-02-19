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
                <div class="page-header">
                    <div class="neighbor-block-to-search">
                        <h3>
                            {{$t('Pages.Exposed_Variables_AvailableVariablesTitle')}}
                        </h3>
                    </div>
                </div>

                <DataTables
                    id="id-table"
                    ref="table"
                    :tableOptions="tableOptions"
                    @cell-clicked="cellAllClicked"
                    noSelect
                    :noPaging="false">
                </DataTables>
            </div>
            <div class="col-sm-6">
                <div class="page-header">
                    <div class="neighbor-block-to-search">
                        <h3>
                            {{$t('Pages.Exposed_Variables_SelectedVariablesTitle')}}
                        </h3>
                    </div>
                </div>
                <table class="table table-striped table-bordered">
                    <thead>
                        <tr>
                            <th>{{this.$t('Pages.ExposedVariables_VariableName')}}</th>
                            <th>{{this.$t('Pages.ExposedVariables_VariableLabel')}}</th>
                            <th>{{this.$t('Pages.ExposedVariables_VariableTitle')}}</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr
                            v-for="variable in exposedVariables"
                            :key="'id' + '__' + variable.id"
                            @click="rowExposedClicked(variable.id)">
                            <td>{{variable.variable}}</td>
                            <td>{{variable.label}}</td>
                            <td>{{variable.title}}</td>
                        </tr>
                    </tbody>
                </table>

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
            <p>{{ $t("Pages.ExposedVariables_ChangeMessage" )}}</p>
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


        <ModalFrame ref="exposedRemoveModal"
            :title="$t('Pages.ConfirmationNeededTitle')"
            :canClose="false">
            <p>{{ $t("Pages.ExposedVariables_RemoveMessage" )}}</p>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-danger"
                    v-bind:disabled="model.isObserving"
                    @click="removeExposedVariable">{{ $t("Common.Ok") }}</button>
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
import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text,  { allowedTags: [], allowedAttributes: [] })

export default {

    data() {

        return {
            exposedVariables: [],
            idToRemove: null,
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
                    {},
                    {
                        data: 'variable',
                        name: 'Variable',
                        title: this.$t('Pages.ExposedVariables_VariableName'),
                        sortable: false,
                    },
                    {
                        data: 'label',
                        name: 'label',
                        title: this.$t('Pages.ExposedVariables_VariableLabel'),
                        sortable: false,
                    },
                    {
                        data: 'title',
                        name: 'Title',
                        title: this.$t('Pages.ExposedVariables_VariableTitle'),
                        sortable: false,
                        'render': function (data, type, row) {
                            return sanitizeHtml(data)
                        },
                    },
                ],
                rowId: function(row) {
                    return row.id
                },
                ajax: {
                    url: this.$config.model.dataUrl + '?id=' + this.$config.model.questionnaireIdentity,
                    type: 'GET',
                    contentType: 'application/json',
                },
                bsort: false,
                responsive: false,
                'ordering': false,
                sDom: 'rf<"table-with-scroll"t>ip',
                createdRow: function(row, data) {
                    if(self.exposedVariables.findIndex(v => v.id === data.id) != -1)
                        $(row).addClass('disabled')
                },
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
                        title: sanitizeHtml(d.title),
                        variable: d.variable,
                    }
                })
            })
    },
    methods: {
        sanitizeHtml: sanitizeHtml,

        saveVariables(){
            this.$refs.exposedChangeModal.modal({keyboard: false})

        },

        async changeExposedStatusSend() {
            const response = await this.$hq.Questionnaire(this.model.questionnaireId, this.model.version)
                .ChangeVariableExposeStatus(this.$config.model.questionnaireIdentity, this.exposedVariables.map(s=>s.id))
            this.$refs.exposedChangeModal.modal('hide')
        },

        cellAllClicked(columnName, rowId, cellData) {
            const parsedRowId = rowId.replace('id__', '')

            var row = this.$refs.table.table.row('#'+rowId)
            if(this.exposedVariables.length >= 15)
                return

            var index = this.exposedVariables.findIndex(x => x.id == parsedRowId)
            if(index === -1){
                for (var i = 0; i < this.exposedVariables.length && this.exposedVariables[i].id < parsedRowId; i++) {/**/}

                var rowData = row.data()
                this.exposedVariables.splice(i, 0, {
                    id: parsedRowId,
                    title: rowData.title,
                    label: rowData.label,
                    variable: rowData.variable,
                })
                row.nodes().to$().addClass('disabled')
            }
        },
        rowExposedClicked(id)
        {
            this.idToRemove = id
            this.$refs.exposedRemoveModal.modal({keyboard: false})

        },
        removeExposedVariable(){
            var index = this.exposedVariables.findIndex(x => x.id == this.idToRemove)
            this.exposedVariables.splice(index,1)

            var row = this.$refs.table.table.row('#'+this.idToRemove)
            if(row != null)
                row.nodes().to$().removeClass('disabled')
            this.$refs.exposedRemoveModal.modal('hide')
        },
    },
}
</script>

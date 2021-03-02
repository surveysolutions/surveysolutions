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
                    noSelect
                    @cell-clicked="cellAllClicked"
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
                            <th style="width: 5%"></th>
                            <th style="width: 35%">
                                {{this.$t('Pages.ExposedVariables_VariableName')}}
                            </th>
                            <!-- <th>{{this.$t('Pages.ExposedVariables_VariableLabel')}}</th> -->
                            <th style="width: 60%">
                                {{this.$t('Pages.ExposedVariables_VariableDisplayTitle')}}
                            </th>

                        </tr>
                    </thead>
                    <tbody>
                        <tr
                            v-for="variable in exposedVariables"
                            :key="'id' + '__' + variable.id">
                            <td>{{ getEntityDisplayType(variable.entityType) }}</td>
                            <td>{{ variable.variable }}</td>
                            <!-- <td>{{ variable.label }}</td> -->
                            <td>{{ getVariableLabel(variable) }}</td>
                            <td>
                                <button class="close"
                                    @click="removeExposedClicked(variable.id)">&times;</button>
                            </td>
                        </tr>
                    </tbody>
                </table>

                <div class="action-buttons">
                    <button
                        @click="saveVariables"
                        :disabled="saveDisabled"
                        class="btn btn-success">
                        {{$t('Common.Save')}}
                    </button>
                </div>
            </div>
        </div>

        <ModalFrame ref="exposedChangeModal"
            :title="$t('Pages.ConfirmationNeededTitle')">
            <p>{{ $t("Pages.ExposedVariables_ChangeMessage" )}}</p>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-success"
                    v-bind:disabled="model.isObserving"
                    @click="changeExposedStatusSend">{{ $t("Common.Save") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>


        <ModalFrame ref="exposedRemoveModal"
            :title="$t('Pages.ConfirmationNeededTitle')">
            <p>{{ $t("Pages.ExposedVariables_RemoveMessage" )}}</p>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-danger"
                    v-bind:disabled="model.isObserving"
                    @click="removeExposedVariable">{{ $t("Common.Remove") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>

    </HqLayout>
</template>

<script>
import { template, map, find, filter, escape } from 'lodash'
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'
import _sanitizeHtml from 'sanitize-html'
const sanitizeHtml = text => _sanitizeHtml(text,  { allowedTags: [], allowedAttributes: [] })


export default {

    data() {

        return {
            exposedVariables: [],
            initialSet: [],
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
                    {
                        data: 'entityType',
                        name: 'EntityType',
                        'width': '5%',
                        sortable: false,
                        'render': function (data, type, row) {
                            return self.getEntityDisplayType(data)
                        },
                    },
                    {
                        data: 'variable',
                        name: 'Variable',
                        'width': '35%',
                        title: this.$t('Pages.ExposedVariables_VariableName'),
                        sortable: false,
                    },
                    // {
                    //     data: 'label',
                    //     name: 'label',
                    //     'width': '15%',
                    //     title: this.$t('Pages.ExposedVariables_VariableLabel'),
                    //     sortable: false,
                    // },
                    {
                        data: 'title',
                        name: 'Title',
                        'width': '55%',
                        title: this.$t('Pages.ExposedVariables_VariableDisplayTitle'),
                        sortable: false,
                        'render': function (data, type, row) {
                            return self.replaceSubstitutions(self.sanitizeHtml(data))
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
        saveDisabled(){
            if(this.initialSet.length !== this.exposedVariables.length)
                return false
            else{
                for(var i = 0; i< this.initialSet.length; i++ )
                {
                    if(this.initialSet[i].id !== this.exposedVariables[i].id)
                        return false
                }
            }
            return true
        },
    },
    mounted() {
        this.$hq.Questionnaire(this.model.questionnaireId, this.model.version)
            .ExposedVariables(this.$config.model.questionnaireIdentity)
            .then(data => {
                this.exposedVariables = map(data, d => {
                    return {
                        id: d.id,
                        title: this.replaceSubstitutions(this.sanitizeHtml(d.title)),
                        variable: d.variable,
                        label: d.label,
                        entityType: d.entityType,
                    }
                })

                this.initialSet = [...this.exposedVariables]
            })
    },
    methods: {
        sanitizeHtml: sanitizeHtml,
        replaceSubstitutions(value){
            if(value == null)
                return null
            return value.replace(/%[\w_]+%/g, '[..]')
        },

        saveVariables(){
            this.$refs.exposedChangeModal.modal({keyboard: false})

        },

        getEntityDisplayType(type){
            switch(type){
                case 'Question':
                    return 'Q'
                case 'Variable':
                    return 'V'
                default:
                    return ''
            }
        },

        async changeExposedStatusSend() {
            const response = await this.$hq.Questionnaire(this.model.questionnaireId, this.model.version)
                .ChangeVariableExposeStatus(this.$config.model.questionnaireIdentity, this.exposedVariables.map(s=>s.id))
            this.initialSet = [...this.exposedVariables]
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
                    id: rowData.id,
                    title: this.replaceSubstitutions(sanitizeHtml(rowData.title)),
                    label: rowData.label,
                    variable: rowData.variable,
                    entityType: rowData.entityType,
                })
                row.nodes().to$().addClass('disabled')
            }
        },
        removeExposedClicked(id)
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
        getVariableLabel(variable){
            return variable.label ? variable.label
                : (variable.title ? this.getDisplayTitle(variable.title) : variable.variable)
        },
        getDisplayTitle(title){
            var transformedTitle = sanitizeHtml(title).replace(/%[\w_]+%/g, '[..]')
            return transformedTitle.length  >= 57 ? transformedTitle.substring(0,54) + '...' : transformedTitle
        },
    },
}
</script>

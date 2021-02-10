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

        <DataTables
            ref="table"
            data-suso="variables-list"
            :tableOptions="tableOptions"
            noSelect
            noSearch
            :noPaging="false">
        </DataTables>


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
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'

export default {

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
                        data: 'id',
                        name: 'Id',
                        title: 'Id',
                        sortable: false,
                    },
                    {
                        data: 'title',
                        name: 'Title',
                        title: this.$t('Pages.Title'),
                        sortable: false,
                    },
                    {
                        data: 'isExposed',
                        name: 'IsExposed',
                        title: this.$t('Workspaces.Name'),
                        sortable: false,
                        'render': function (data, type, row) {
                            if (data === true) {
                                return '<input type="checkbox" id="chkadd_' + row.id + '" checked value="true">'
                            }
                            else {
                                return '<input type="checkbox" id="chkadd_' + row.id + '" >'
                            }
                        },
                    },
                ],
                rowId: function(row) {
                    return row.id
                },
                ajax: {
                    url: this.$config.model.dataUrl + '?id='+this.$config.model.questionnaireIdentity,
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
    },
    mounted() {},
    methods: {
        exposedVariableChanged() {
            this.$refs.exposedChangeModal.modal({
                backdrop: 'static',
                keyboard: false,
            })
        },

        async changeExposedStatusSend() {
            const response = await this.$hq.Questionnaire(this.model.questionnaireId, this.model.version)
                .ChangeVariableExposedStatus( 19, true)
            this.$refs.exposedChangeModal.modal('hide')
        },
    },
}
</script>

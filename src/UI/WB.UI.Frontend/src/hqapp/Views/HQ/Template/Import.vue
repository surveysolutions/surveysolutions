<template>
    <HqLayout :title="$config.model.title"
        :hasFilter="false">
        <template slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="this.$config.model.surveySetup">
                        {{this.$t('MainMenu.SurveySetup')}}
                    </a>
                </li>
                <li>
                    <a :href="this.$config.model.import">
                        {{this.$t('QuestionnaireImport.ListOfMyQuestionnaires')}}
                    </a>
                </li>
            </ol>
            <h1>{{this.$t('ImportQuestionnaire.PageHeader')}}</h1>
            <div class="signed-name">
                {{this.$t('ImportQuestionnaire.SignedInBlock', {user: this.$config.model.designerUserName })}}

                <a :href="this.$config.model.logoutFromDesigner">
                    {{this.$t('ImportQuestionnaire.SignOut')}}
                </a>
            </div>
        </template>
        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            tableClass='import-interview'></DataTables>
    </HqLayout>
</template>

<script>
import escape from 'lodash'
import { DateFormats } from '~/shared/helpers'
import moment from 'moment'

export default {
    computed: {
        tableOptions() {
            var self = this
            return {
                columns: [
                    {
                        data: 'title',
                        name: 'Title',
                        title: this.$t('ImportQuestionnaire.Table_Title'),
                        render: function (data, type, row) {
                            return `<a href="${self.$config.model.importMode}/${row.id}">${escape(data)}</a>`
                        },
                    },
                    {
                        data: 'lastModified',
                        name: 'LastEntryDate',
                        'class': 'changed-recently',
                        title: this.$t('ImportQuestionnaire.Table_LastModified'),
                        render: function(data) {
                            if (data === null || data === undefined || data === '')
                                return ''
                            const utcDate = moment.utc(data)
                            return utcDate.local().format(DateFormats.dateTime)
                        },
                    },
                    {
                        data: 'createdBy',
                        name: 'CreatorName',
                        title: this.$t('ImportQuestionnaire.Table_CreatedBy'),
                        'class': 'created-by',
                    },
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                sDom: 'rf<"table-with-scroll"t>ip',
                order: [[1, 'desc']],
                bInfo: false,
                footer: true,
                responsive: false,
            }
        },
    },
}
</script>

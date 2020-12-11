<template>
    <HqLayout :hasFilter="false"
        tag="audit-log-page"
        :title="$t('AuditLog.PageTitle')" >
        <DataTables
            :tableOptions="tableOptions"
            noSearch
            exportable>
        </DataTables>
    </HqLayout>
</template>

<script>
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'

export default {
    computed: {
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'logDate',
                        name: 'LogDate',
                        title: this.$t('AuditLog.LogDate'),
                        render: function(data) {
                            return self.formatUtcDate(data)
                        },
                    },
                    {
                        data: 'userName',
                        name: 'UserName',
                        title: this.$t('AuditLog.User'),
                    },
                    {
                        data: 'type',
                        name: 'Type',
                        title: this.$t('AuditLog.EventType'),
                    },
                    {
                        data: 'log',
                        name: 'Log',
                        title: this.$t('AuditLog.Log'),
                    },
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                order: [[0, 'desc']],
            }
        },
    },
    methods: {
        formatUtcDate(date) {
            const momentDate = moment.utc(date)
            return momentDate.local().format(DateFormats.dateTime)
        },
    },
}
</script>

<style>

</style>

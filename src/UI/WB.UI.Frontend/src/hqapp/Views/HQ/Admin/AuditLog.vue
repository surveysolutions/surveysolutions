<template>
    <HqLayout :hasFilter="false" tag="audit-log-page" :title="$t('AuditLog.PageTitle')">
        <DataTables :tableOptions="tableOptions" noSearch exportable>
        </DataTables>
    </HqLayout>
</template>

<script>
import { DateFormats } from '~/shared/helpers'
import moment from 'moment'
import DataTables from 'datatables.net'

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
                        width: '20%',
                        title: this.$t('AuditLog.LogDate'),
                        render: function (data) {
                            return self.formatUtcDate(data)
                        },
                    },
                    {
                        data: 'userName',
                        name: 'UserName',
                        width: '10%',
                        title: this.$t('AuditLog.User'),
                        render: DataTables.render.text()
                    },
                    {
                        data: 'type',
                        name: 'Type',
                        orderable: false,
                        width: '10%',
                        title: this.$t('AuditLog.EventType'),
                        render: DataTables.render.text()
                    },
                    {
                        data: 'log',
                        name: 'Log',
                        title: this.$t('AuditLog.Log'),
                        render: DataTables.render.text()
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

<style></style>

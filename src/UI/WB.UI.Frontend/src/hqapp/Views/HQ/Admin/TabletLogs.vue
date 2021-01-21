<template>
    <HqLayout :hasFilter="false"
        tag="tablet-logs-page"
        :title="$t('TabletLogs.PageTitle')"
        :subtitle="$t('TabletLogs.PageSubTitle')">
        <DataTables ref="table"
            :tableOptions="tableOptions"
            :contextMenuItems="contextMenuItems"
            noSearch></DataTables>
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
                        data: 'deviceId',
                        name: 'DeviceId',
                        title: this.$t('TabletLogs.DeviceId'),
                    },
                    {
                        data: 'userName',
                        name: 'UserName',
                        title: this.$t('TabletLogs.UserName'),
                    },
                    {
                        data: 'receiveDateUtc',
                        name: 'ReceiveDateUtc',
                        title: this.$t('TabletLogs.ReceiveDateUtc'),
                        render: function(data) {
                            return self.formatUtcDate(data)
                        },
                    },
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                order: [[2, 'desc']],
            }
        },
    },
    methods: {
        contextMenuItems({rowData, rowIndex}) {
            const self = this
            const menu = []
            menu.push({
                name: self.$t('Common.Download'),
                callback: () => {
                    window.open(rowData.downloadUrl, '_blank')
                },
            })
            return menu
        },
        formatUtcDate(date) {
            const momentDate = moment.utc(date)
            return momentDate.local().format(DateFormats.dateTime)
        },
    },
}
</script>

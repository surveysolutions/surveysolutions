<template>
    <DataTables ref="table" :tableOptions="tableOptions"></DataTables>
</template>

<script>
import {DateFormats} from '~/shared/helpers'
import moment from 'moment'

export default {
    data() {
        return {}
    },
    computed: {
        tableOptions() {
            var self = this
            let columns = [
                {
                    data: 'androidId',
                    title: this.$t('ControlPanel.DeviceId')
                },
                {
                    data: 'creationDate',
                    title: this.$t('ControlPanel.UploadDate'),
                     render: function(data, type, row) {
                        return new moment(data).format(DateFormats.dateTime)
                    },
                },
                {
                    data: 'userName',
                    title: this.$t('Users.UserName')
                },
                {
                    data: 'userId',
                    title: 'Id'
                },
                {
                    data: 'size',
                    title: 'Size'
                }
            ]

            return {
                columns: columns,
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                bInfo: false,
                responsive: false
            }
        },
    },
}
</script>

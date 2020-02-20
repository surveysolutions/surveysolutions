<template>
    <HqLayout :hasFilter="false"
        :title="title"
        :topicButtonRef="this.model.createUrl"
        :topicButton="$t('Users.AddAPIUser')">
        <div slot='subtitle'>
            <div class="neighbor-block-to-search">
                <ol class="list-unstyled">
                    <li>{{ $t('Pages.Users_API_Instruction1') }}</li>
                    <li>{{ $t('Pages.Users_API_Instruction2') }}</li>
                </ol>
            </div>
        </div>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            @ajaxComplete="onTableReload"
            noSelect
            :noPaging="false">
        </DataTables>

    </HqLayout>
</template>

<script>

import moment from 'moment'
import { formatNumber } from './formatNumber'
import { DateFormats } from '~/shared/helpers'

export default {
    data() {
        return {
            usersCount : '',
        }
    },
    mounted() {
        this.loadData()
    },
    methods: {
        loadData() {
            if (this.$refs.table){
                this.$refs.table.reload()
            }
        },
        onTableReload(data) {
            this.usersCount = formatNumber(data.recordsTotal)
        },
    },
    computed: {
        model() {
            return this.$config.model
        },
        title() {
            return this.$t('Users.ApiUsersCountDescription', {count: this.usersCount})
        },
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'userName',
                        name: 'UserName',
                        title: this.$t('Users.UserName'),
                        className: 'nowrap',
                        render: function(data, type, row) {
                            return `<a href='${self.model.editUrl}/${row.userId}'>${data}</a>`
                        },
                    },
                    {
                        data: 'creationDate',
                        name: 'CreationDate',
                        className: 'date',
                        title: this.$t('Users.CreationDate'),
                        tooltip: this.$t('Users.AccountCreationDateTooltip'),
                        render: function(data, type, row) {
                            var localDate = moment.utc(data).local()
                            return localDate.format(DateFormats.dateTimeInList)
                        },
                    },
                    {
                        data: 'isLocked',
                        name: 'IsLockedByHQ',
                        className: 'date',
                        title: this.$t('Users.Locked'),
                    },
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
    },
}
</script>

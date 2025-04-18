<template>
    <HqLayout :hasFilter="false" :title="title" :topicButtonRef="this.model.createUrl"
        :topicButton="$t('Users.AddHeadquarters')">
        <template v-slot:subtitle>
            <div class="neighbor-block-to-search">
                <ol v-if="this.model.showInstruction" class="list-unstyled">
                    <li>{{ $t('Pages.Users_Headquarters_Instruction1') }}</li>
                    <li>{{ $t('Pages.Users_Headquarters_Instruction2') }}</li>
                </ol>
                <ol v-if="user.isObserver && !user.isObserving" class="list-unstyled">
                    <li>{{ $t('Pages.Observer_Memo1') }}</li>
                    <li>{{ $t('Pages.Observer_Memo2') }}</li>
                </ol>
            </div>
        </template>

        <DataTables ref="table" :tableOptions="tableOptions" @ajaxComplete="onTableReload"
            :contextMenuItems="contextMenuItems" :supportContextMenu="model.showContextMenu" noSelect :noPaging="false">
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
            usersCount: '',
        }
    },
    mounted() {
        this.loadData()
    },
    methods: {
        loadData() {
            if (this.$refs.table) {
                this.$refs.table.reload()
            }
        },
        onTableReload(data) {
            this.usersCount = formatNumber(data.recordsTotal)
        },
        contextMenuItems({ rowData, rowIndex }) {
            if (!this.model.showContextMenu)
                return null

            const self = this
            const menu = []
            menu.push({
                name: self.$t('Users.ImpersonateAsUser'),
                callback: () => {
                    const link = self.model.impersonateUrl + '?personName=' + rowData.userName
                    window.open(link, '_blank')
                },
            })
            return menu
        },
    },
    computed: {
        model() {
            return this.$config.model
        },
        user() {
            return this.model.currentUser
        },
        title() {
            return this.$t('Users.HeadquartersCountDescription', { count: this.usersCount })
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
                        render: function (data, type, row) {
                            return `<a href='${self.model.editUrl}/${row.userId}'>${data}</a>`
                        },
                    },
                    {
                        data: 'creationDate',
                        name: 'CreationDate',
                        className: 'date',
                        title: this.$t('Users.CreationDate'),
                        tooltip: this.$t('Users.AccountCreationDateTooltip'),
                        render: function (data, type, row) {
                            var localDate = moment.utc(data).local()
                            return localDate.format(DateFormats.dateTimeInList)
                        },
                    },
                    {
                        data: 'email',
                        name: 'Email',
                        className: 'date',
                        title: this.$t('Users.HeadquartersEmail'),
                        render: function (data, type, row) {
                            return data ? '<a href=\'mailto:' + data + '\'>' + data + '</a>' : ''
                        },
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

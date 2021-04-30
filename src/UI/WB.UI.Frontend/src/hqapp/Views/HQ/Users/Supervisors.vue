<template>
    <HqLayout :hasFilter="false">
        <div slot="subtitle">
            <div class="neighbor-block-to-search">
                <div class="topic-with-button">
                    <h1>{{ $t('Users.SupervisorsCountDescription', {count: this.usersCount}) }}</h1>
                </div>
                <ol v-if="!user.isObserver && !user.isObserving"
                    class="list-unstyled">
                    <li>{{ $t('Pages.Users_Supervisors_Instruction2') }}</li>
                </ol>
            </div>
        </div>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :contextMenuItems="contextMenuItems"
            :supportContextMenu="user.isObserver && !user.isObserving"
            selectableId="userId"
            @selectedRowsChanged="rows => selectedSupervisors = rows"
            @totalRows="(rows) => usersCount = rows"
            @page="resetSelection"
            mutliRowSelect
            :noPaging="false"></DataTables>

    </HqLayout>
</template>

<script>
import moment from 'moment'
import { DateFormats } from '~/shared/helpers'

export default {
    data() {
        return {
            usersCount: 0,
            selectedSupervisors: [],
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
        contextMenuItems({rowData, rowIndex}) {
            if (!this.user.isObserver || this.user.isObserving) return null

            const self = this
            const menu = []
            menu.push({
                name: self.$t('Users.ImpersonateAsUser'),
                callback: () => {
                    const link = self.api.impersonateUrl + '?personName=' + rowData.userName
                    window.open(link, '_blank')
                },
            })
            return menu
        },
        resetSelection() {
            this.selectedSupervisors.splice(0, this.selectedSupervisors.length)
        },
    },
    computed: {
        model() {
            return this.$config.model
        },
        user() {
            return this.model.currentUser
        },
        api() {
            return this.model.api
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
                        orderable: true,
                        className: 'nowrap',
                        render: function(data, type, row) {
                            return row.isArchived ? data : `<a href='/users/Manage/${row.userId}'>${data}</a>`
                        },
                    },
                    {
                        data: 'creationDate',
                        name: 'CreationDate',
                        className: 'date',
                        title: this.$t('Users.CreationDate'),
                        tooltip: this.$t('Users.AccountCreationDateTooltip'),
                        orderable: true,
                        render: function(data, type, row) {
                            var localDate = moment.utc(data).local()
                            return localDate.format(DateFormats.dateTimeInList)
                        },
                    },
                    {
                        data: 'email',
                        name: 'Email',
                        className: 'date',
                        title: this.$t('Users.SupervisorEmail'),
                        orderable: true,
                        render: function(data, type, row) {
                            return data ? '<a href=\'mailto:' + data + '\'>' + data + '</a>' : ''
                        },
                    },
                    {
                        data: 'isArchived',
                        name: 'IsArchived',
                        title: this.$t('Users.ArchivingStatusTitle'),
                        tooltip: this.$t('Users.ArchivingStatusTitle'),
                        orderable: true,
                        render: function(data, type, row) {
                            return data ? self.$t('Common.Yes') : self.$t('Common.No')
                        },
                    },
                ],
                ajax: {
                    url: this.api.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
        hasSelectedSupervisors() {
            return this.selectedSupervisors.length > 0
        },
    },
}
</script>

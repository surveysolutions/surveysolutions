<template>
    <HqLayout :hasFilter="false">
        <div slot="subtitle">
            <div class="neighbor-block-to-search">
                <div class="topic-with-button">
                    <h1>{{ $t('Users.SupervisorsCountDescription', {count: this.usersCount}) }}</h1>
                    <a
                        v-if="!user.isObserver"
                        class="btn btn-success"
                        :href="api.createUrl"
                    >{{ $t('Users.AddSupervisor') }}</a>
                </div>
                <ol v-if="!user.isObserver && !user.isObserving" class="list-unstyled">
                    <li>{{ $t('Pages.Users_Supervisors_Instruction1') }}</li>
                    <li>{{ $t('Pages.Users_Supervisors_Instruction2') }}</li>
                </ol>
            </div>
        </div>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            @ajaxComplete="onTableReload"
            :contextMenuItems="contextMenuItems"
            :supportContextMenu="user.isObserver"
            noSelect
        ></DataTables>
    </HqLayout>
</template>

<script>
import moment from 'moment'

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
            this.usersCount = data.recordsTotal
        },
        contextMenuItems({rowData, rowIndex}) {
            if (!this.user.isObserver) return null

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
    },
    computed: {
        model() {
            return this.$config.model
        },
        user(){
            return this.model.currentUser
        },
        api(){
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
                            if (self.user.isObserver) return data
                            else return `<a href='${self.api.editUrl}/${row.userId}'>${data}</a>`
                        },
                    },
                    {
                        data: 'creationDate',
                        name: 'CreationDate',
                        className: 'date',
                        title: this.$t('Users.CreationDate'),
                        orderable: true,
                        render: function(data, type, row) {
                            var localDate = moment.utc(data).local()
                            return localDate.format(window.CONFIG.dateFormat)
                        },
                    },
                    {
                        data: 'email',
                        name: 'Email',
                        className: 'date',
                        title: this.$t('Users.SupervisorEmail'),
                        orderable: true,
                        render: function(data, type, row) {
                            return data ? "<a href='mailto:" + data + "'>" + data + '</a>' : ''
                        },
                    },
                    {
                        data: 'isArchived',
                        name: 'IsArchived',
                        title: this.$t('Users.ArchivingStatusTitle'),
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
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
    },
}
</script>

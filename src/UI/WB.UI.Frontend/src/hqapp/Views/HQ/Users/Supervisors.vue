<template>
    <HqLayout :hasFilter="false">
        <div slot="subtitle">
            <div class="neighbor-block-to-search">
                <div class="topic-with-button">
                    <h1>{{ $t('Users.SupervisorsCountDescription', {count: this.usersCount}) }}</h1>
                    <a
                        v-if="this.model.showAddUser"
                        class="btn btn-success"
                        :href="this.model.createUrl + '/supervisor'"
                    >{{ $t('Users.AddSupervisor') }}</a>
                </div>
                <ol v-if="this.model.showInstruction" class="list-unstyled">
                    <li>{{ $t('Pages.Users_Supervisors_Instruction1') }}</li>
                    <li>{{ $t('Pages.Users_Supervisors_Instruction2') }}</li>
                </ol>
            </div>
        </div>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :addParamsToRequest="addParamsToRequest"
            @ajaxComplete="onTableReload"
            :contextMenuItems="contextMenuItems"
            exportable
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
        addParamsToRequest(requestData) {
            requestData.search = (this.questionnaireId || {}).key
        },
        onTableReload(data) {
            this.usersCount = data.recordsTotal
        },
        contextMenuItems({rowData, rowIndex}) {
            if (!this.model.showContextMenu) return []

            const self = this
            const menu = []
            menu.push({
                name: self.$t('Users.ImpersonateAsUser'),
                callback: () => {
                    const link = self.model.impersonateUrl + '?personName=' + rowData.userName
                    //window.location.href = link
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
        description() {
            return this.model.reportNameDescription
        },
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'userName',
                        name: "UserName",
                        title: this.$t('Users.UserName'),
                        orderable: true,
                        className: 'nowrap',
                        render: function(data, type, row) {
                            return `<a href='${self.model.editUrl}/${row.userId}'>${data}</a>`
                        },
                    },
                    {
                        data: 'creationDate',
                        name: "CreationDate",
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
                        name: "Email",
                        className: 'date',
                        title: this.$t('Users.SupervisorEmail'),
                        orderable: true,
                        render: function(data, type, row) {
                            return data ? "<a href='mailto:" + data + "'>" + data + '</a>' : ''
                        },
                    },
                    {
                        data: 'isArchived',
                        name: "IsArchived",
                        title: this.$t('Users.ArchivingStatusTitle'),
                        orderable: true,
                        render: function(data, type, row) {
                            return data ? self.$t('Common.Yes') : self.$t('Common.No')
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

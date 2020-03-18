<template>
    <HqLayout :hasFilter="false">
        <div slot="subtitle">
            <div class="neighbor-block-to-search">
                <div class="topic-with-button">
                    <h1>{{ $t('Users.SupervisorsCountDescription', {count: this.usersCount}) }}</h1>
                    <a
                        v-if="!user.isObserver"
                        class="btn btn-success"
                        :href="api.createUrl">{{ $t('Users.AddSupervisor') }}</a>
                </div>
                <ol v-if="!user.isObserver && !user.isObserving"
                    class="list-unstyled">
                    <li>{{ $t('Pages.Users_Supervisors_Instruction1') }}</li>
                    <li>{{ $t('Pages.Users_Supervisors_Instruction2') }}</li>
                </ol>
            </div>
        </div>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :contextMenuItems="contextMenuItems"
            :supportContextMenu="user.isObserver && !user.isObserving"
            :selectable="user.isAdministrator"
            selectableId="userId"
            @selectedRowsChanged="rows => selectedSupervisors = rows"
            @totalRows="(rows) => usersCount = rows"
            @page="resetSelection"
            mutliRowSelect
            :noPaging="false"></DataTables>
        <Confirm
            ref="confirmArchive"
            id="confirmArchive"
            slot="modals">{{$t('Pages.Supervisors_ArchiveSupervisorsConfirmMessage')}}</Confirm>
        <Confirm ref="confirmUnarchive"
            id="confirmUnarchive"
            slot="modals">
            {{$t('Archived.UnarchiveSupervisorWarning')}}
            <br />
            {{$t('Pages.Supervisors_UnarchiveSupervisorsConfirm')}}
        </Confirm>

        <div class="panel panel-table"
            v-if="user.isAdministrator && hasSelectedSupervisors">
            <div class="panel-body">
                <input
                    class="double-checkbox-white"
                    id="q1az"
                    type="checkbox"
                    checked
                    disabled="disabled"/>
                <label for="q1az">
                    <span class="tick"></span>
                    <span>{{selectedSupervisors.length}} {{$t('Pages.Supervisors_Selected')}}</span>
                </label>
                <button
                    type="button"
                    class="btn btn-default btn-danger"
                    @click="archiveSupervisors">{{$t('Pages.Supervisors_Archive')}}</button>
                <button
                    type="button"
                    class="btn btn-default btn-success"
                    @click="unArchiveSupervisors">{{$t('Pages.Supervisors_Unarchive')}}</button>
            </div>
        </div>
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
        async archiveSupervisorsAsync(isArchive) {
            await this.$http.post(this.api.archiveUsersUrl, {
                archive: isArchive,
                userIds: this.selectedSupervisors,
            })

            this.loadData()
        },
        archiveSupervisors() {
            var self = this
            this.$refs.confirmArchive.promt(async ok => {
                if (ok) await self.archiveSupervisorsAsync(true)
            })
        },
        unArchiveSupervisors() {
            var self = this
            this.$refs.confirmUnarchive.promt(async ok => {
                if (ok) await self.archiveSupervisorsAsync(false)
            })
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
                            return row.isArchived ? data : `<a href='${self.api.editUrl}/${row.userId}'>${data}</a>`
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

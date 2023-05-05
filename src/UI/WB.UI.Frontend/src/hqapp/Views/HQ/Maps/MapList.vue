<template>
    <HqLayout :hasFilter="false" :title="$t('Pages.MapList_Title')">
        <div slot="headers">
            <div class="topic-with-button">
                <h1>{{ $t('Pages.MapList_Title') }}</h1>
                <label class="btn btn-success btn-file" v-if="actionsAlowed">
                    {{ $t('Pages.MapList_Upload') }}
                    <input accept=".zip" ref="uploader" id="File" name="File" @change="onFileChange" type="file" value="" />
                </label>
            </div>
            <div ref="status">
                <p>{{ statusMessage }}</p>
            </div>
            <div ref="errors" class="alert alert-danger">
                <div v-for="error in errorList" :key="error">
                    {{ error }}
                </div>
            </div>
            <ol class="list-unstyled" v-if="!isSupervisor">
                <li>{{ $t('Pages.MapList_UploadDescription') }} </li>
                <li>{{ $t('Pages.MapList_UploadDescriptionExtra') }}</li>
            </ol>
            <p v-if="!isSupervisor">
                <a :href="$config.model.userMapsUrl">{{ $t('Pages.MapList_UserMapsLink') }}</a>
            </p>
            <p v-if="!isSupervisor">
                <a :href="$config.model.userMapLinkingUrl">{{ $t('Pages.MapList_UserLinking') }}</a>
            </p>
        </div>
        <DataTables ref="table" :tableOptions="tableOptions" :contextMenuItems="contextMenuItems">
        </DataTables>

        <Confirm ref="confirmDiscard" id="discardConfirm" :okTitle="$t('Common.Delete')" okClass="btn-danger" slot="modals">
            {{ deleteDialogBody }}
        </Confirm>
    </HqLayout>
</template>

<script>

import { DateFormats, humanFileSize } from '~/shared/helpers'
import moment from 'moment'
import * as toastr from 'toastr'
import gql from 'graphql-tag'
const query = gql`query MapsList($workspace: String!, $order: [MapsSort!], $skip: Int, $take: Int, $where: MapsFilter) {
  maps(workspace: $workspace, order: $order, skip: $skip, take: $take, where: $where) {
    totalCount
    filteredCount
    nodes {
      fileName
      importDateUtc
      size
    }
  }
}`

export default {
    data: function () {
        return {
            statusMessage: '',
            errorList: [],
            deleteMapName: '',
        }
    },
    mounted() {
        this.reload()
    },
    methods: {
        updateStatus(newMessage, errors) {
            this.statusMessage = this.$t('Pages.Map_Status') + ': ' + newMessage
            if (errors != null) {
                this.errorList = errors
            }
            else
                this.errorList = []
        },
        progressStyle() {
            return {
                width: this.fileProgress + '%',
            }
        },
        reload() {
            if (this.$refs.table)
                this.$refs.table.reload()
        },
        onFileChange(e) {
            const files = e.target.files || e.dataTransfer.files

            if (!files.length) {
                return
            }

            const statusupdater = this.updateStatus
            const reloader = this.reload
            const uploadingMessage = this.$t('Pages.Map_Uploading')
            const uploadingErrorMessage = this.$t('Pages.Map_UploadingError')
            const uploadingSuccess = this.$t('Pages.Map_UploadingSuccess')
            const uploadingFileTooBig = this.$t('Pages.Map_UploadingFileTooBig')

            const fd = new FormData()
            var fileToUpload = this.$refs.uploader.files[0]

            var filesize = ((fileToUpload.size / 1024) / 1024).toFixed(4)

            if (filesize >= 1024) {
                statusupdater(uploadingFileTooBig)
                return
            }

            fd.append('file', fileToUpload)

            $.ajax({
                url: this.$config.model.uploadMapsFileUrl,
                xhr() {
                    const xhr = $.ajaxSettings.xhr()
                    xhr.upload.onprogress = (e) => {
                        statusupdater(uploadingMessage + ' ' + parseInt((e.loaded / e.total) * 100) + '%')
                    }
                    return xhr
                },
                data: fd,
                processData: false,
                contentType: false,
                type: 'POST',
                success: function (data) {
                    if (!data.isSuccess)
                        statusupdater(uploadingErrorMessage, data.errors)
                    else
                        statusupdater(uploadingSuccess)
                    reloader()
                },
                error: function (err) {
                    statusupdater(uploadingErrorMessage)
                },
            })
            this.$refs.uploader.value = ''
        },

        contextMenuItems({ rowData }) {
            const self = this
            return [
                {
                    name: this.$t('Common.Open'),
                    callback: () => window.location = self.$hq.basePath + 'Maps/Details?mapname=' + encodeURIComponent(rowData.fileName),
                },
                {
                    name: this.$t('Pages.MapList_DeleteMap'),
                    callback: () => this.confirmDeleteMap(rowData.fileName),
                },
            ]
        },
        confirmDeleteMap(fileName) {
            const self = this
            this.deleteMapName = fileName
            this.$refs.confirmDiscard.promt(ok => {
                if (ok) {
                    self.$apollo.mutate({
                        mutation: gql`
                                mutation deleteMap($workspace: String!, $fileName: String!) {
                                    deleteMap(workspace: $workspace, fileName: $fileName) {
                                        fileName
                                    }
                                }`,
                        variables: {
                            'fileName': fileName,
                            workspace: self.$store.getters.workspace,
                        },
                    }).then(response => {
                        self.$refs.table.reload()
                    }).catch(err => {
                        console.error(err)
                        toastr.error(err.message.toString())
                    })
                }
            })
        },
    },
    computed: {
        config() {
            return this.$config.model
        },
        actionsAlowed() {
            return !this.config.isObserver && !this.config.isObserving && !this.config.isSupervisor
        },
        isSupervisor() {
            return this.config.isSupervisor
        },
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'fileName',
                        name: 'FileName',
                        class: 'title',
                        title: this.$t('Pages.MapList_MapName'),
                    },
                    {
                        data: 'fileName',
                        name: 'MapType',
                        class: 'parameters',
                        title: this.$t('Pages.MapList_MapType'),
                        render(data) {
                            const icon_name = data.endsWith('.shp') ? 'shapefile_icon.svg' : 'map_icon.svg'
                            return `<img src="/img/${icon_name}" width="20px"></img>`
                        },
                    },
                    {
                        data: 'size',
                        name: 'Size',
                        class: 'parameters',
                        title: this.$t('Pages.MapList_Size'),
                        render(data) {
                            return humanFileSize(data, false)
                        },
                    },
                    {
                        data: 'importDateUtc',
                        name: 'ImportDateUtc',
                        class: 'date',
                        title: this.$t('Pages.MapList_Updated'),
                        render(data) {
                            return moment
                                .utc(data)
                                .local()
                                .format(DateFormats.dateTimeInList)
                        },
                    },
                ],
                pageLength: 20,
                ajax(data, callback, settings) {
                    const order = {}
                    const order_col = data.order[0]
                    const column = data.columns[order_col.column]

                    order[column.data] = order_col.dir.toUpperCase()

                    const variables = {
                        order: order,
                        skip: data.start,
                        take: data.length,
                        workspace: self.$store.getters.workspace,
                    }

                    const where = {
                        and: [],
                    }

                    const search = data.search.value

                    if (search && search != '') {
                        where.and.push({
                            or: [
                                { fileName: { startsWith: search } }],
                        })
                    }

                    if (where.and.length > 0) {
                        variables.where = where
                    }

                    self.$apollo.query({
                        query,
                        variables: variables,
                        fetchPolicy: 'network-only',
                    }).then(response => {
                        const data = response.data.maps
                        self.totalRows = data.totalCount
                        self.filteredCount = data.filteredCount
                        callback({
                            recordsTotal: data.totalCount,
                            recordsFiltered: data.filteredCount,
                            draw: ++this.draw,
                            data: data.nodes,
                        })
                    }).catch(err => {
                        callback({
                            recordsTotal: 0,
                            recordsFiltered: 0,
                            data: [],
                            error: err.toString(),
                        })
                        console.error(err)
                        toastr.error(err.message.toString())
                    })
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
        deleteDialogBody() {
            return this.$t('Pages.Map_DiscardConfirm', { map: this.deleteMapName })
        },
    },
}
</script>

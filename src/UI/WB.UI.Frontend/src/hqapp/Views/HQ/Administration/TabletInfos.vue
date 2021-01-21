<template>
    <HqLayout tag="tablet-infos-page">
        <div class="row">
            <form
                method="post"
                ref="frm"
                class="topic-with-button"
                enctype="multipart/form-data">
                <label class="btn btn-success btn-file">
                    {{ $t('Pages.Upload_Upload') }}
                    <input
                        ref="uploader"
                        name="file"
                        @change="onFileChange"
                        type="file"
                        accept=".zip"
                        value=""/>
                </label>
            </form>
            <DataTables ref="table"
                :tableOptions="tableOptions"></DataTables>
        </div>
    </HqLayout>
</template>

<script>
import { DateFormats, humanFileSize } from '~/shared/helpers'
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
                    title: this.$t('Pages.PackagesInfo_DeviceId'),
                },
                {
                    data: 'creationDate',
                    title: this.$t('Pages.PackagesInfo_UploadDate'),
                    render: function(data, type, row) {
                        return new moment(data).format(DateFormats.dateTime)
                    },
                },
                {
                    data: 'userName',
                    title: this.$t('Pages.PackagesInfo_UserName'),
                },
                {
                    data: 'userId',
                    title: this.$t('Pages.PackagesInfo_UserId'),
                },
                {
                    data: 'size',
                    title: this.$t('Pages.PackagesInfo_Size'),
                    render: function(data, _, row) {
                        return `<a href=${row.downloadUrl}>${humanFileSize(
                            data
                        )} <span class="glyphicon glyphicon-download"></span></a>`
                    },
                },
            ]

            return {
                columns: columns,
                ajax: {
                    url: `${this.$hq.basePath}api/ControlPanelApi/TabletInfos`,
                    type: 'GET',
                    contentType: 'application/json',
                },
                bInfo: false,
                responsive: false,
            }
        },
    },
    methods: {
        onFileChange() {
            this.$refs.frm.submit()
        },
    },
}
</script>

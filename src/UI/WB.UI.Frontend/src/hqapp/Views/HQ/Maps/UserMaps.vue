<template>
    <HqLayout :hasFilter="false">
        <template v-slot:headers>
            <div>
                <ol class="breadcrumb">
                    <li>
                        <a :href="$config.model.mapsUrl">{{ $t("Pages.MapList_Title") }}</a>
                    </li>
                </ol>
                <h1>{{ $t("Pages.MapList_UserMapsTitle") }}</h1>
                <p>
                    <a :href="$config.model.userMapLinkingUrl">{{ $t('Pages.MapList_UserLinking') }}</a>
                </p>
            </div>
        </template>
        <DataTables ref="table" :tableOptions="tableOptions"></DataTables>
        <template v-slot:models>
            <Confirm ref="confirmDiscard" id="discardConfirm">{{ $t("Pages.Map_DiscardConfirm") }}</Confirm>
        </template>
    </HqLayout>
</template>

<script>
import { map, join, escape } from 'lodash'

export default {
    mounted() {
        if (this.$refs.table) {
            this.$refs.table.reload()
        }
    },
    methods: {
        reload() {
            this.$refs.table.reload()
        },
    },
    computed: {
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                select: {
                    style: 'api',
                    info: false,
                },
                columns: [
                    {
                        data: 'userName',
                        name: 'UserName',
                        class: 'title',
                        title: this.$t('Pages.MapList_Name'),
                    },
                    {
                        data: 'maps',
                        name: 'Maps',
                        orderable: false,
                        searchable: false,
                        title: this.$t('Pages.MapList_Title'),
                        render(data) {
                            const mapsLinks = map(data, fileName => {
                                return (
                                    '<a href=\'' +
                                    self.$hq.basePath +
                                    'Maps/Details?mapname=' +
                                    encodeURIComponent(fileName) +
                                    '\'>' +
                                    escape(fileName) +
                                    '</a>'
                                )
                            })

                            return join(mapsLinks, ', ')
                        },
                    },
                ],
                ajax: {
                    url: self.$config.model.dataUrl,
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

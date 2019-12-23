<template>
    <HqLayout :title="$config.model.title" :subtitle="$config.model.subTitle" :hasFilter="false">
        <DataTables
            ref="table"
            :tableOptions="tableOptions"
        ></DataTables>
    </HqLayout>
</template>
<script>

import { DateFormats } from "~/shared/helpers"
import moment from "moment"

export default {
    methods: {
        
    },
    computed: {
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'title',
                        title: this.$t('Dashboard.Title'),
                        name: 'Title',
                        className: 'without-break changed-recently'
                    },
                    {
                        data: 'version',
                        title: this.$t('Dashboard.Version')
                    },
                    {
                        data: "importDate",
                        name: "ImportDate",
                        title: this.$t('Dashboard.ImportDate'),
                        render: function(data, type, row) {
                            return new moment(data).format(DateFormats.dateTime)
                        }
                    },
                    {
                        data: "lastEntryDate",
                        name: "LastEntryDate",
                        "class": "date",
                        title: this.$t('Dashboard.LastEntryDate'),
                        render: function(data, type, row) {
                            return new moment(data).format(DateFormats.dateTime)
                        }
                    },
                    {
                        data: "creationDate",
                        name: "CreationDate",
                        "class": "date",
                        title: this.$t('Dashboard.CreationDate'),
                        render: function(data, type, row) {
                            return new moment(data).format(DateFormats.dateTime)
                        }
                    },
                    {
                        data: "webModeEnabled",
                        name: "WebModeEnabled",
                        "class": "parameters",
                        "orderable": false,
                        title: this.$t('Dashboard.WebMode'),
                        render: function(data, type, row) {
                            return data === true ? self.$t('Common.Yes') : self.$t('Common.No')
                        }
                    }
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                    contentType: 'application/json',
                },
                sDom: 'rf<"table-with-scroll"t>ip',
                order: [[2, 'desc']],
                bInfo: false,
                footer: true,
                responsive: false
            }
        }
    }
}
</script>

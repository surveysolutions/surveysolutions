<template>
    <HqLayout :hasFilter="false" >
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="$config.model.mapsUrl">{{$t("Pages.MapList_Title")}}</a>
                </li>
            </ol>
            <h1>{{$config.model.fileName}}</h1>
        </div>
        <div class="row">
            <div class="col-md-9 col-sm-9">
                <div class="row">
                    <div class="col-md-4 col-sm-4 questionnaire-statistics">
                        <ul class="list-unstyled">
                            <li v-if="!$config.model.shapeType"><strong>{{ $t("Pages.MapDetails_MaxScale") }}:</strong> <span>{{$config.model.maxScale}}</span></li>
                            <li v-if="!$config.model.shapesCount"><strong>{{ $t("Pages.MapDetails_MinScale") }}:</strong> <span>{{$config.model.minScale}}</span></li>
                            <li v-if="$config.model.shapeType"><strong>{{ $t("Pages.MapDetails_ShapeType") }}:</strong> <span>{{$config.model.shapeType}}</span></li>
                            <li v-if="$config.model.shapesCount"><strong>{{ $t("Pages.MapDetails_ShapesCount") }}:</strong> <span>{{$config.model.shapesCount}}</span></li>
                        </ul>
                    </div>
                    <div class="col-md-4 col-sm-4 questionnaire-statistics">
                        <ul class="list-unstyled">
                            <li><strong>{{ $t("Pages.MapDetails_Size") }} :</strong> <span>{{$config.model.size}}</span></li>
                            <li><strong>Wkid:</strong> <span>{{$config.model.wkid}}</span></li>
                        </ul>
                    </div>
                    <div class="col-md-4 col-sm-4 questionnaire-statistics">
                        <ul class="list-unstyled">
                            <li><strong>{{ $t("Pages.MapDetails_ImportedOn") }}:</strong> <span>{{importDate}}</span></li>
                            <li v-if="$config.model.uploadedBy"><strong>{{ $t("Pages.MapDetails_UploadedBy") }}:</strong> <span>{{$config.model.uploadedBy}}</span></li>
                        </ul>
                    </div>
                </div>
                <div class="row"
                    v-if="$config.model.duplicateMapLabels.length > 0">
                    <div class="col-md-12 col-sm-12 questionnaire-statistics">
                        <p style="color:red">{{ $t("Pages.MapDetails_DuplicateLabelsWarning") }} </p>
                        <ul class="list-unstyled">
                            <li v-for="item in $config.model.duplicateMapLabels"
                                :key="item.label">
                                <strong>{{ item.label }}</strong> - <span>{{ item.count }}</span>
                            </li>
                        </ul>
                    </div>
                </div>
                <iframe
                    title="Map preview" 
                    width="100%"
                    height="550px"
                    :src="$config.model.mapPreviewUrl"></iframe>
                <p>{{ mapDisclaimer }} </p>
            </div>

            <div id="list"
                class="col-md-3 col-sm-3">
                <DataTables ref="table"
                    :tableOptions="tableOptions"
                    :addParamsToRequest="addParamsToRequest"
                    :contextMenuItems="contextMenuItems">
                </DataTables>

                <Confirm ref="confirmDiscard"
                    id="discardConfirm"
                    slot="modals">
                    {{ $t("Pages.MapUserLink_DiscardConfirm") }}
                </Confirm>
            </div>
        </div>
    </HqLayout>
</template>

<script>

import {orderBy} from 'lodash'
import * as toastr from 'toastr'
import gql from 'graphql-tag'
import {DateFormats} from '~/shared/helpers'
import moment from 'moment-timezone'

export default {
    mounted() {
        this.reload()
    },
    methods: {
        reload() {
            if (this.$refs.table){
                this.$refs.table.reload()
            }
        },
        addParamsToRequest(requestData) {
            requestData.mapName = this.$config.model.fileName
        },
        contextMenuItems({ rowData }) {
            return [{
                name: this.$t('Pages.MapDetails_DelinkUser'),
                callback: () => { this.delinkUserFromMap(rowData.userName, this.$config.model.fileName)},
            }]
        },

        delinkUserFromMap(userName, fileName)
        {
            const self = this
            this.$refs.confirmDiscard.promt(ok =>
            {
                if (ok)
                {
                    self.$apollo.mutate({
                        mutation: gql`
                                mutation deleteUserFromMap($workspace: String!, $fileName: String!, $userName: String!) {
                                    deleteUserFromMap(workspace: $workspace, fileName: $fileName, userName: $userName) {
                                        fileName
                                    }
                                }`,
                        variables: {
                            'fileName' : fileName,
                            'userName': userName,
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
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                language: {
                    emptyTable: this.$t('Pages.Map_NoUsers'),
                },
                columns: [
                    {
                        data: 'userName',
                        name: 'UserName', // case-sensitive!
                        'class': 'title',
                        title: this.$t('Pages.MapDetails_InterviewerName'),
                        orderable: true,
                    },
                ],
                ajax (data, callback, settings) {
                    const order_col = data.order[0]
                    const column = data.columns[order_col.column]

                    const query = gql`query MapDetailsQuery($workspace: String!,  $fileName: String!) {
                                        maps(workspace: $workspace, where: {
                                                fileName: {
                                                    eq: $fileName
                                                }
                                                }) {
                                            nodes {
                                                users {
                                                    userName
                                                }
                                            }
                                        }
                                    }`

                    self.$apollo.query({
                        query,
                        variables: {
                            'fileName' : self.$config.model.fileName,
                            workspace: self.$store.getters.workspace,
                        },
                        fetchPolicy: 'network-only',
                    }).then(response => {
                        const users = response.data.maps.nodes[0].users
                        const orderedUsers = orderBy(users, [column.data], [order_col.dir])

                        self.totalRows = users.length
                        self.filteredCount = users.length
                        callback({
                            recordsTotal: self.totalRows,
                            recordsFiltered: self.filteredCount,
                            draw: ++this.draw,
                            data: orderedUsers,
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
        mapDisclaimer() {
            if (this.$config.model.shapesCount)
            {
                return this.$config.model.isPreviewGeoJson
                    ? this.$t('Pages.MapDetails_SimplifiedShapefilesDisclaimer')
                    : this.$t('Pages.MapDetails_FullShapefilesDisclaimer')
            }
            return this.$t('Pages.MapDetails_MapDisclaimer')
        },
        importDate() {
            var date = moment.utc(this.$config.model.importDate)
            return date.local().format(DateFormats.dateTime)
        },
    },

}
</script>

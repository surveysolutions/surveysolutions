<template>
    <HqLayout :hasFilter="true" :hasHeader="true" :fixedWidth="false">
        <Filters slot="filters" title="">
            <FilterBlock :title="$t('Common.Properties')">
                <ul class="list-group small">
                    <li class="list-group-item pointer" v-if="!$config.model.shapeType"><strong>{{
                        $t("Pages.MapDetails_MaxScale") }}:</strong> <span>{{ $config.model.maxScale }}</span></li>
                    <li class="list-group-item pointer" v-if="!$config.model.shapesCount"><strong>{{
                        $t("Pages.MapDetails_MinScale") }}:</strong> <span>{{ $config.model.minScale }}</span></li>
                    <li class="list-group-item pointer" v-if="$config.model.shapeType"><strong>{{
                        $t("Pages.MapDetails_ShapeType") }}:</strong> <span>{{ $config.model.shapeType }}</span></li>
                    <li class="list-group-item pointer" v-if="$config.model.shapesCount"><strong>{{
                        $t("Pages.MapDetails_ShapesCount") }}:</strong> <span>{{ $config.model.shapesCount }}</span>
                    </li>
                    <li class="list-group-item pointer"><strong>{{ $t("Pages.MapDetails_Size") }} :</strong>
                        <span>{{ $config.model.size }}</span>
                    </li>
                    <li class="list-group-item pointer"><strong>Wkid:</strong> <span>{{ $config.model.wkid }}</span></li>
                    <li class="list-group-item pointer"><strong>{{ $t("Pages.MapDetails_ImportedOn") }}:</strong>
                        <span>{{ importDate }}</span>
                    </li>
                    <li class="list-group-item pointer" v-if="$config.model.uploadedBy"><strong>{{
                        $t("Pages.MapDetails_UploadedBy") }}:</strong> <span>{{ $config.model.uploadedBy }}</span></li>
                </ul>
            </FilterBlock>
            <FilterBlock :title="$t('Common.LinkUser')">
                <div>
                    <Typeahead style="margin-left: 10px;" control-id="newLikedUserId" :placeholder="$t('Common.LinkUser')"
                        :value="newLikedUserId" :ajax-params="{}" @selected="newLinkedUserSelected"
                        :fetch-url="config.api.users"></Typeahead>
                </div>
                <div style="margin-top: 5px;">
                    <button type="button" class="btn btn-primary" role="confirm" @click="linkUserToMap"
                        :disabled="!newLikedUserId">{{ $t("Common.LinkUser") }}</button>
                </div>
            </FilterBlock>
            <FilterBlock :title="$t('Common.LinkedUsers')">
                <div id="list">
                    <DataTables ref="table" :tableOptions="tableOptions" :addParamsToRequest="addParamsToRequest"
                        :contextMenuItems="contextMenuItems">
                    </DataTables>

                    <Confirm ref="confirmDiscard" id="discardConfirm" slot="modals">
                        <p>{{ $t("Pages.MapUserLink_DiscardConfirm") }} </p>
                        <p>{{ $t("Pages.MapUserLink_DiscardConfirm1") }} </p>
                    </Confirm>
                </div>
            </FilterBlock>
        </Filters>
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="$config.model.mapsUrl">{{ $t("Pages.MapList_Title") }}</a>
                </li>
            </ol>
            <h1>{{ $config.model.fileName }}</h1>
        </div>
        <div style="display: flex; width: 100%; height: 100%; flex-direction: column;">
            <iframe style="flex-grow: 1; border: none; margin: 0; padding: 0; min-height: 550px;" id="map-iframe"
                :src="$config.model.mapPreviewUrl"></iframe>
            <p> {{ mapDisclaimer }} </p>
        </div>

    </HqLayout>
</template>

<script>

import { orderBy } from 'lodash'
import * as toastr from 'toastr'
import gql from 'graphql-tag'
import { DateFormats } from '~/shared/helpers'
import moment from 'moment-timezone'
import FilterBlock from '../../../components/FilterBlock.vue'

export default {
    data() {
        return {
            newLikedUserId: null,
        }
    },
    mounted() {
        this.reload();
    },
    methods: {
        reload() {
            if (this.$refs.table) {
                this.$refs.table.reload();
            }
        },
        addParamsToRequest(requestData) {
            requestData.mapName = this.$config.model.fileName;
        },
        contextMenuItems({ rowData }) {
            return [{
                name: this.$t("Pages.MapDetails_DelinkUser"),
                callback: () => { this.delinkUserFromMap(rowData.userName, this.$config.model.fileName); },
            }];
        },
        delinkUserFromMap(userName, fileName) {
            const self = this;
            this.$refs.confirmDiscard.promt(ok => {
                if (ok) {
                    self.$apollo.mutate({
                        mutation: gql`
                                mutation deleteUserFromMap($workspace: String!, $fileName: String!, $userName: String!) {
                                    deleteUserFromMap(workspace: $workspace, fileName: $fileName, userName: $userName) {
                                        fileName
                                    }
                                }`,
                        variables: {
                            "fileName": fileName,
                            "userName": userName,
                            workspace: self.$store.getters.workspace,
                        },
                    }).then(response => {
                        self.$refs.table.reload();
                    }).catch(err => {
                        console.error(err);
                        toastr.error(err.message.toString());
                    });
                }
            });
        },
        newLinkedUserSelected(newValue) {
            this.newLikedUserId = newValue
        },
        linkUserToMap() {

            if (this.newLikedUserId) {
                const self = this;
                const fileName = this.$config.model.fileName;
                const userName = this.newLikedUserId.value;

                self.$apollo.mutate({
                    mutation: gql`
                                mutation AddUserToMap($workspace: String!, $fileName: String!, $userName: String!) {
                                    addUserToMap(workspace: $workspace, fileName: $fileName, userName: $userName) {
                                        fileName
                                    }
                                }`,
                    variables: {
                        "fileName": fileName,
                        "userName": userName,
                        workspace: self.$store.getters.workspace,
                    },
                }).then(response => {
                    self.$refs.table.reload();
                }).catch(err => {
                    console.error(err);
                    toastr.error(err.message.toString());
                });

                this.newLikedUserId = null;
            }
        }
    },
    computed: {
        config() {
            return this.$config.model;
        },
        tableOptions() {
            var self = this;
            return {
                deferLoading: 0,
                language: {
                    emptyTable: this.$t("Pages.Map_NoUsers"),
                },
                columns: [
                    {
                        data: "userName",
                        name: "UserName",
                        "class": "title",
                        title: this.$t("Pages.MapDetails_InterviewerName"),
                        orderable: true,
                    },
                ],
                ajax(data, callback, settings) {
                    const order_col = data.order[0];
                    const column = data.columns[order_col.column];
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
                                    }`;
                    self.$apollo.query({
                        query,
                        variables: {
                            "fileName": self.$config.model.fileName,
                            workspace: self.$store.getters.workspace,
                        },
                        fetchPolicy: "network-only",
                    }).then(response => {
                        const users = response.data.maps.nodes[0].users;
                        const orderedUsers = orderBy(users, [column.data], [order_col.dir]);
                        self.totalRows = users.length;
                        self.filteredCount = users.length;
                        callback({
                            recordsTotal: self.totalRows,
                            recordsFiltered: self.filteredCount,
                            draw: ++this.draw,
                            data: orderedUsers,
                        });
                    }).catch(err => {
                        callback({
                            recordsTotal: 0,
                            recordsFiltered: 0,
                            data: [],
                            error: err.toString(),
                        });
                        console.error(err);
                        toastr.error(err.message.toString());
                    });
                },
                responsive: false,
                filter: false,
                searching: false,
                order: [[0, "asc"]],
                sDom: "rf<\"table-with-scroll\"t>ip",
            };
        },
        mapDisclaimer() {
            if (this.$config.model.shapesCount) {
                return this.$config.model.isPreviewGeoJson
                    ? this.$t("Pages.MapDetails_SimplifiedShapefilesDisclaimer")
                    : this.$t("Pages.MapDetails_FullShapefilesDisclaimer");
            }
            return this.$t("Pages.MapDetails_MapDisclaimer");
        },
        importDate() {
            var date = moment.utc(this.$config.model.importDate);
            return date.local().format(DateFormats.dateTime);
        },
    },
    components: { FilterBlock }
}
</script>

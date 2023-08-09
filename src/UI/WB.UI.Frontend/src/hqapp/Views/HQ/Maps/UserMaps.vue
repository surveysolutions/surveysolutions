<template>
    <HqLayout :hasFilter="false">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="$config.model.mapsUrl">{{$t("Pages.MapList_Title")}}</a>
                </li>
            </ol>
            <h1>{{$t("Pages.MapList_UserMapsTitle")}}</h1>
            <p >
                <a :href="$config.model.userMapLinkingUrl">{{$t('Pages.MapList_UserLinking')}}</a>
            </p>
        </div>
        <DataTables ref="table"
            :tableOptions="tableOptions"></DataTables>

        <Confirm
            ref="confirmDiscard"
            id="discardConfirm"
            slot="modals">{{ $t("Pages.Map_DiscardConfirm") }}</Confirm>
    </HqLayout>
</template>

<script>
import {map, join, uniqBy, filter, some, orderBy} from 'lodash'
import * as toastr from 'toastr'
import gql from 'graphql-tag'
const query = gql`query UserMaps($workspace: String!, $order: [MapsSort!], $skip: Int, $take: Int, $where: MapsFilter) {
  maps(workspace: $workspace, order: $order, skip: $skip, take: $take, where: $where) {
    totalCount,
    filteredCount,
    nodes {
      fileName
      users {
          userName
      }
    }
  }
}`

export default {
    data: function() {
        return {
            statusMessage: '',
            errorList: [],
        }
    },
    mounted() {
        if (this.$refs.table) {
            this.$refs.table.reload()
        }
    },
    methods: {
        updateStatus(newMessage, errors) {
            this.statusMessage = this.$t('Pages.Map_Status') + ': ' + newMessage
            if (errors != null) {
                this.errorList = errors
            } else this.errorList = []
        },
        progressStyle() {
            return {
                width: this.fileProgress + '%',
            }
        },
        reload() {
            this.$refs.table.reload()
        },
    },
    computed: {
        config() {
            return this.$config.model
        },
        actionsAlowed() {
            return !this.config.isObserver && !this.config.isObserving
        },
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
                            const mapsLinks = map(data, map => {
                                return (
                                    '<a href=\'' +
                                    self.$hq.basePath +
                                    'Maps/Details?mapname=' +
                                    encodeURIComponent(map.fileName) +
                                    '\'>' +
                                    map.fileName +
                                    '</a>'
                                )
                            })

                            return join(mapsLinks, ', ')
                        },
                    },
                ],
                ajax (data, callback, settings) {
                    const order = {}
                    const order_col = data.order[0]
                    const column = data.columns[order_col.column]

                    order[column.data] = order_col.dir.toUpperCase()

                    const variables = {
                        skip: data.start,
                        take: data.length,
                        workspace: self.$store.getters.workspace,
                    }

                    const where = {
                        and: [],
                    }

                    const search = data.search.value

                    if(search && search != '') {
                        where.and.push({ or: [
                            {fileName : {startsWith : search }},
                            {users : {some : {userName : {startsWith: search.toLowerCase()}}}},
                        ],
                        })
                    }

                    if(where.and.length > 0) {
                        variables.where = where
                    }

                    self.$apollo.query({
                        query,
                        variables: variables,
                        fetchPolicy: 'network-only',
                    }).then(response => {
                        const nodes = response.data.maps.nodes

                        const interviewers = uniqBy(map(nodes, 'users').flat(), 'userName')
                        const sortedInterviewers = orderBy(interviewers, [column.data], [order_col.dir])

                        const rows = map(sortedInterviewers, function(inter){
                            return {
                                userName: inter.userName,
                                maps: filter(nodes, function(map){
                                    return some(map.users, { userName: inter.userName})
                                }),
                            }
                        })

                        self.totalRows = interviewers.length
                        self.filteredCount = interviewers.length
                        callback({
                            recordsTotal: self.totalRows,
                            recordsFiltered: self.filteredCount,
                            draw: ++this.draw,
                            data: rows,
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
    },
}
</script>

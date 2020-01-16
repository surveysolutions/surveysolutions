<template>
  <HqLayout :hasFilter="false" :title="title" :topicButtonRef="this.model.createUrl" :topicButton="$t('Users.AddHeadquarters')">
    <div slot='subtitle'>
        <div class="neighbor-block-to-search">
            <ol v-if="this.model.showInstruction" class="list-unstyled">
                <li>{{ $t('Pages.Users_Headquarters_Instruction1') }}</li>
                <li>{{ $t('Pages.Users_Headquarters_Instruction2') }}</li>
            </ol>
        </div>
    </div>

    <DataTables
      ref="table"
      :tableOptions="tableOptions"
      @ajaxComplete="onTableReload"
      :contextMenuItems="contextMenuItems"
      :supportContextMenu="model.showContextMenu"
      exportable
      noSelect
    >
    </DataTables>

  </HqLayout>
</template>

<script>

import moment from "moment";

export default {
    data() {
        return {
            usersCount : ''
        }
    },
    mounted() {
        this.loadData()
    },
    methods: {
        loadData() {
            if (this.$refs.table){
                this.$refs.table.reload();
            }
        },
        onTableReload(data) {
            this.usersCount = data.recordsTotal
        },
        contextMenuItems({rowData, rowIndex}) {
            if (!this.model.showContextMenu)
                return null;

            const self = this
            const menu = []
            menu.push({
                name: self.$t('Users.ImpersonateAsUser'),
                callback: () => {
                    const link = self.model.impersonateUrl + '?personName=' + rowData.userName
                    //window.location.href = link
                    window.open(link, "_blank")
                }
            })
            return menu
        },
    },
    computed: {
        model() {
            return this.$config.model;
        },
        title() {
            return this.$t('Users.HeadquartersCountDescription').replace('{0}', this.usersCount)
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
                        data: "userName",
                        title: this.$t("Users.UserName"),
                        orderable: true,
                        className: "nowrap",
                        render: function(data, type, row) {
                            return `<a href='${self.model.editUrl}/${row.userId}'>${data}</a>`;
                        }
                    },
                    {
                        data: "creationDate",
                        className: "date",
                        title: this.$t("Users.CreationDate"),
                        orderable: false,
                        render: function(data, type, row) {
                            var localDate = moment.utc(data).local();
                            return localDate.format(window.CONFIG.dateFormat);
                        }
                    },
                    {
                        data: "email",
                        className: "date",
                        title: this.$t("Users.HeadquartersEmail"),
                        orderable: false,
                        render: function(data, type, row) {
                            return data ? "<a href='mailto:" + data + "'>" + data + "</a>" : "";
                        }
                    }
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: "GET",
                    contentType: 'application/json'
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip'
            }
        }
    }
}
</script>

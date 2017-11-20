<template>
    <HqLayout :hasFilter="false" :title="$t('MainMenu.Maps')">        
        <div slot="headers">
            <div class="topic-with-button" >
                <h1>{{$t('MainMenu.Maps')}}</h1>
                <form :action="$config.model.uploadMapUrl" enctype="multipart/form-data" id="MapsUploadForm" method="post">
                    <label class="btn btn-success btn-file">
                        Upload .zip file
                        <input accept=".zip" id="File" name="File" onchange="this.form.submit()" type="file" value="" />
                    </label>
                </form>
            </div>
            <ol class="list-unstyled">
                <li>Upload zip archive containing maps. </li>
                <li>Files with same name will be overridden.</li>
            </ol>
            <p>
                <a :href="$config.model.userMapLinkingUrl">Update user to maps linking</a>
            </p>
        </div>
        <DataTables ref="table" 
            :tableOptions="tableOptions" 
            :contextMenuItems="contextMenuItems">
        </DataTables>

        <Confirm ref="confirmDiscard"
                 id="discardConfirm"
                 slot="modals">
            {{ $t("Pages.Map_DiscardConfirm") }}
        </Confirm>

    </HqLayout>
</template>

<script>
export default {
  mounted() {
    this.reload();
  },
  methods: {
    reload() {
            this.$refs.table.reload();
        },
    contextMenuItems({ rowData }) {
      return [
        {
          name: this.$t("Common.Open"),
          callback: () => this.$store.dispatch("openMap", rowData.fileName)
        },
        {
          name: this.$t("Dashboard.DeleteMap"),
          callback: () => this.deleteMap(rowData.fileName)
        }
      ];
    },
    deleteMap(fileName) {
        const self = this;
            this.$refs.confirmDiscard.promt(ok => {
                if (ok) {
                    this.$http({
                        method: 'delete',
                        url: this.config.deleteMapLinkUrl,
                        data: {map: fileName}});

                     self.$refs.table.reload();
                }
            });
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
        columns: [
          {
            data: "fileName",
            name: "FileName",
            class: "title",
            title: this.$t("Maps.Name")
          },
          {
            data: "size",
            name: "Size",
            class: "parameters",
            title: this.$t("Maps.Size")
          },
          {
            data: "importDate",
            name: "ImportDate",
            class: "date",
            title: this.$t("Maps.UpdateDate")
          }
        ],
        ajax: {
          url: this.$config.model.dataUrl,
          type: "GET"
        },
        responsive: false,
        order: [[0, "asc"]],
        sDom: 'rf<"table-with-scroll"t>ip'
      };
    }
  }
};
</script>

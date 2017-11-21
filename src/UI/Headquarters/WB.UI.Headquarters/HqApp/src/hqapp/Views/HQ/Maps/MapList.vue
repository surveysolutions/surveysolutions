<template>
    <HqLayout :hasFilter="false" :title="$t('Pages.MapList_Title')">        
        <div slot="headers">
            <div class="topic-with-button" >
                <h1>{{$t('Pages.MapList_Title')}}</h1>
                <form :action="$config.model.uploadMapUrl" enctype="multipart/form-data" id="MapsUploadForm" method="post" v-if="actionsAlowed">
                    <label class="btn btn-success btn-file">
                        {{$t('Pages.MapList_Upload')}}
                        <input accept=".zip" id="File" name="File" onchange="this.form.submit()" type="file" value="" />
                    </label>
                </form>
            </div>
            <ol class="list-unstyled">
                <li>{{$t('Pages.MapList_UploadDescription')}} </li>
                <li>{{$t('Pages.MapList_UploadDescriptionExtra')}}</li>
            </ol>
            <p>
                <a :href="$config.model.userMapLinkingUrl">{{$t('Pages.MapList_UserLinking')}}</a>
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
          name: this.$t("Pages.MapList_DeleteMap"),
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
    actionsAlowed() {
            return !this.config.isObserver && !this.config.isObserving;
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
            title: this.$t("Pages.MapList_Name")
          },
          {
            data: "size",
            name: "Size",
            class: "parameters",
            title: this.$t("Pages.MapList_Size")
          },
          {
            data: "importDate",
            name: "ImportDate",
            class: "date",
            title: this.$t("Pages.MapList_Updated")
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

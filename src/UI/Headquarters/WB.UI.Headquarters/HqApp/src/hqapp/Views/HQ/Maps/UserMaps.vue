<template>
    <HqLayout :hasFilter="false" :title="$t('Pages.MapList_Title')">        
        
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
    data: function(){
        return {        
            statusMessage: '',
            errorList: []
        }        
    },
  mounted() {
    this.$refs.table.reload();
  },
  methods: {
      updateStatus(newMessage, errors){
          this.statusMessage = this.$t("Pages.Map_Status") + ": " + newMessage;
          if(errors != null){
            this.errorList = errors;
          }
          else
            this.errorList = [];
      },
      progressStyle() {
                return {
                    width: this.fileProgress + "%"
                }
            },
    reload() {
            this.$refs.table.reload();
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
            data: "userName",
            name: "UserName",
            class: "title",
            title: this.$t("Pages.MapList_Name")
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

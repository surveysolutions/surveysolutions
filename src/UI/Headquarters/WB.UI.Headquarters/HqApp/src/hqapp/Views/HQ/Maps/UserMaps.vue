<template>
    <HqLayout :hasFilter="false" >        
        <div slot="headers">
                <ol class="breadcrumb">
                    <li>
                        <a :href="$config.model.mapsUrl">{{$t("Pages.MapList_Title")}}</a>
                    </li>
                </ol>
                    <h1>{{$t("Pages.MapList_UserMapsTitle")}}</h1> 
                <p>
                    <a :href="$config.model.userMapLinkingUrl">{{$t('Pages.MapList_UserLinking')}}</a>
                </p>                   
        </div>   
        <DataTables ref="table" 
            :tableOptions="tableOptions">
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
        select: {
            style: 'api',
            info: false
        },
        columns: [
          {
            data: "userName",
            name: "UserName",
            class: "title",
            title: this.$t("Pages.MapList_Name")
          },
          {
            data: "maps",
            name: "Maps",
            orderable: false,
            searchable: false,
            title: this.$t("Pages.MapList_Title"),
            render(data) {
                const mapsLinks = _.map(data, (map) => { 
                    return "<a href='" + window.input.settings.config.basePath + "Maps/Details?mapname=" + encodeURIComponent(map) + "'>" + map + "</a>" });

                return _.join(mapsLinks, ", ");
            }            
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

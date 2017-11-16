<template>
    <HqLayout :hasFilter="false" :title="$config.model.fileName">
        
        <div class="row">
          <div class="col-md-7 col-sm-12">
                <div class="row">
                    <div class="col-md-4 col-sm-3 questionnaire-statistics">
                        <ul class="list-unstyled">
                            <li><b>{{ $t("Pages.MapDetails_MaxScale") }}:</b> <span>{{$config.model.maxScale}}</span></li>
                            <li><b>{{ $t("Pages.MapDetails_MinScale") }}:</b> <span>{{$config.model.minScale}}</span></li>
                        </ul>
                    </div>
                    <div class="col-md-4 col-sm-3 questionnaire-statistics">
                        <ul class="list-unstyled">
                            <li><b>{{ $t("Pages.MapDetails_Size") }} :</b> <span>{{$config.model.size}}</span></li>
                            <li><b>Wkid:</b> <span>{{$config.model.wkid}}</span></li>
                        </ul>
                    </div>
                    <div class="col-md-4 col-sm-3 questionnaire-statistics">
                        <ul class="list-unstyled">
                            <li><b>{{ $t("Pages.MapDetails_ImportedOn") }}:</b> <span>{{$config.model.importDate}}</span></li>
                        </ul>
                    </div>
                </div>
                <iframe width="100%" height="550px" :src="$config.model.mapPreviewUrl"></iframe>
                <p>{{ $t("Pages.MapDetails_MapDisclaimer") }} </p>
      </div>
    
      <div id="list" class="col-md-5 col-sm-6">
        <DataTables ref="table" 
            :tableOptions="tableOptions"
            :addParamsToRequest="addParamsToRequest"
            :contextMenuItems="contextMenuItems">
        </DataTables>
      </div>
      </div>
    </HqLayout>
</template>

<script>
export default {
    mounted() {
        this.$refs.table.reload();
    },
    methods: {        
        addParamsToRequest(requestData) {
            requestData.mapName = this.$config.model.fileName;
        },
        contextMenuItems({ rowData }) {
            return [{
                name: this.$t("Pages.MapDetails_DelinkUser"),
                callback: () => self.delinkUserFromMap(rowData)
                    
            }];
        },
        delinkUserFromMap(userName) {
            const self = this;
            this.$refs.confirmDiscard.promt(ok => {
                if (ok) {
                    
                    self.$store.dispatch("delinkUser", {
                        userName : userName,
                        mapName: this.$config.model.fileName,
                        callback: self.reload
                    });
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
                        data: "userName",
                        name: "UserName", // case-sensitive!
                        "class": "title",
                        title: this.$t("Pages.MapDetails_InterviewerName"),
                        orderable: true                       
                    }                    
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: "GET"
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip'                
            }
        }
    }

}
</script>

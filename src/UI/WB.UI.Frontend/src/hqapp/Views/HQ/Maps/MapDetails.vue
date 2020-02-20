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
                <iframe width="100%"
                    height="550px"
                    :src="$config.model.mapPreviewUrl"></iframe>
                <p>{{ $t("Pages.MapDetails_MapDisclaimer") }} </p>
            </div>
    
            <div id="list"
                class="col-md-5 col-sm-6">
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
                    this.$http({
                        method: 'delete',
                        url: this.$config.model.deleteMapUserLinkUrl,
                        data: {user:userName, map: fileName}})

                    this.$refs.table.reload()
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
                columns: [
                    {
                        data: 'userName',
                        name: 'UserName', // case-sensitive!
                        'class': 'title',
                        title: this.$t('Pages.MapDetails_InterviewerName'),
                        orderable: true,                       
                    },                    
                ],
                ajax: {
                    url: this.$config.model.dataUrl,
                    type: 'GET',
                },
                responsive: false,
                order: [[0, 'asc']],
                sDom: 'rf<"table-with-scroll"t>ip',                
            }
        },
    },

}
</script>

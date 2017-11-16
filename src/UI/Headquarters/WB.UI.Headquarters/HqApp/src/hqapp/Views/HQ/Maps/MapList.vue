<template>
    <HqLayout :hasFilter="false">
              
        <DataTables ref="table" 
            :tableOptions="tableOptions">
        </DataTables>
      
      
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
                        "class": "title",
                        title: this.$t("Maps.Name"),
                    },
                    {
                        data: "size",
                        name: "Size",
                        "class": "parameters",
                        title: this.$t("Maps.Size")
                    },
                    {
                        data: "importDate",
                        name: "ImportDate",
                        "class": "date",
                        title: this.$t("Maps.UpdateDate")
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

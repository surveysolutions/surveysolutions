<template>

    <div class="question table-view scroller" :id="hash">


        <ag-grid-vue 
            class="ag-theme-customStyles"
            :defaultColDef="defaultColDef"
            :columnDefs="columnDefs"
            :rowData="rowData"

            rowHeight="40"
            domLayout='autoHeight'
            headerHeight="50"

            @grid-ready="onGridReady"
            @onCellEditingStopped="endCellEditting"
            >
        </ag-grid-vue>

    </div>
</template>

<script lang="js">
    import { entityDetails } from "../mixins"
    import { GroupStatus } from "./index"
    import { debounce, every, some } from "lodash"
    import { AgGridVue } from "ag-grid-vue";

    export default {
        name: 'TableRoster',
        mixins: [entityDetails],
        
        data() {
            return {
                defaultColDef: null,
                columnDefs: null,
                rowData: null,
                gridApi: null,
                columnApi: null,
            }
        },

        components: {
            AgGridVue
        },

        beforeMount() {
            this.columnDefs = [
                {headerName: "Flip", field: "rosterTitle", pinned: true, editable: false},
                {headerName: "In which month of [...] did this enterprise begin operations?", field: "month", cellClass: function(params) { return (params.value==='04 - April' || params.value =='' ? '' :'has-warnings'); }},
                {headerName: "Which country/countrise operate in (i.e. have sales in)?", 
                    field: 'country',
                    cellEditor: 'agSelectCellEditor',
                    cellEditorParams: {
                        values: ['Ukraine', 'Latvia', 'Macedonia', 'Romania', '(other)']
                    },
                    cellClass: function(params) { return (params.value==='Romania' ? 'disabled-question' :''); }
                },
                {headerName: "In which country/countries does this enterprise have registered entities?", field: "registration", cellClass: function(params) { return (params.value==='Latvia' || params.value =='' ? '' :'has-error'); }},
                {headerName: "Is one or more of the enterprise founders from the region in which you operate?", field: "enterprise", cellClass: function(params) { return (params.value == null ? 'not-applicable' :''); }},
                {headerName: "When your enterprise became an NGO?", 
                    field: "NGO", 
                    editable: true, 
                    cellEditor: 'datePicker'
                }
            ];

            this.defaultColDef = {
                // set every column width
                width: 220,
                height: 38,
                resizable: true,
                // make every column editable
                editable: true
            };
            
            this.rowData = [
                {rosterTitle: 'Banchee', month: "04 - April", country: "Ukraine", registration: "Latvia", enterprise: "Yes", NGO: "24/08/2008"},
                {rosterTitle: 'Banchee', month: "", country: "Ukraine", registration: "Latvia", enterprise: "Yes", NGO: "24/08/2008"},
                {rosterTitle: 'Banchee', month: "04 - April", country: "Ukraine", registration: "Latvia", enterprise: "Yes", NGO: "24/08/2008"},
                {rosterTitle: 'Banchee', month: "04 - April", country: "Romania", registration: "Latvia", enterprise: "Yes", NGO: "24/08/2008"},
                {rosterTitle: 'Banchee', month: "04 - April", country: "Ukraine", registration: "Latvia", enterprise: "Yes", NGO: "24/08/2008"},
                {rosterTitle: 'Banchee', month: "06 - April", country: "Ukraine", registration: "Latvia", enterprise: "Yes", NGO: "24/08/2008"},
                {rosterTitle: 'Banchee', month: "04 - April", country: "Ukraine", registration: "Macedonia", enterprise: "Yes", NGO: "24/08/2008"},
                {rosterTitle: 'Banchee', month: "04 - April", country: "Ukraine", registration: "Latvia", enterprise: "Yes", NGO: "24/08/2008"},
                {rosterTitle: 'Banchee', month: "04 - April", country: "", registration: "Latvia", enterprise: "Yes", NGO: "24/08/2008"},
                {rosterTitle: 'Banchee', month: "04 - April", country: 'Ukraine', registration: "Latvia", NGO: "24/08/2008"},
                {rosterTitle: 'Banchee', month: "04 - April", country: "Ukraine", registration: "Latvia", enterprise: "Yes", NGO: "24/08/2008"}
            ];
        },

        watch: {
            ["$store.getters.scrollState"]() {
                 this.scroll();
            }
        },

        mounted() {
            this.scroll();
        },

        computed: {
            statusClass() {
                return ['table-view scroller', 'scroller']
            }           
        },
        methods : {
            onGridReady(params) {
                this.gridApi = params.api;
                this.columnApi = params.columnApi;
            },

            doScroll: debounce(function() {
                if(this.$store.getters.scrollState ==  this.id){
                    window.scroll({ top: this.$el.offsetTop, behavior: "smooth" })
                    this.$store.dispatch("resetScroll")
                }
            }, 200),

            scroll() {
                if(this.$store && this.$store.state.route.hash === "#" + this.id) {
                    this.doScroll(); 
                }
            },

            endCellEditting(event) {
			    console.log('cellEditingStopped, value:' + event.value);
		    }
        }
    }
</script>

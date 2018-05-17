<template>
    <div :class="wrapperClass">
        <table ref="table"
               class="table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews responsive">
            <thead><slot name="header"></slot></thead>
            <tbody></tbody>
        </table>
        <div class="download-report-as"
             v-if="exportable">
            {{$t("Pages.DownloadReport")}}
            <a target="_blank"
               v-bind:href="this.export.excel">XLSX</a>, <a target="_blank"
               v-bind:href="this.export.csv">CSV</a> {{$t("Pages.Or")}}
            <a target="_blank"
               v-bind:href="this.export.tab">TAB</a>
        </div>
        <slot />
    </div>
</template>

<script>

import 'datatables.net'
import 'datatables.net-select'
import 'jquery-contextmenu'
import 'jquery-highlight'
import './datatable.plugins'

$.fn.dataTable.ext.errMode = 'throw';

var checkBox =
    _.template(
        '<input class="checkbox-filter" type="checkbox" value="<%= id %>"' +
        ' id="<%= checkboxId %>"><label for="<%= checkboxId %>">' +
        '<span class="tick"></span></label>');

export default {
    name: 'DataTable',
    props: {
        addParamsToRequest: {
            type: Function,
            default(d) { return d; }
        },
        multiorder: {
            type: Boolean, default: false
        },
        wrapperClass: Object,
        responseProcessor: {
            type: Function,
            default(r) { return r; }
        },
        tableOptions: {
            type: Object,
            default() { return {}; }
        },
        hasTotalRow: {
            type: Boolean, default: false
        },
        contextMenuItems: {
            type: Function
        },
        pageLength: {
            type: Number,
            default: 20
        },
        authorizedUser: { type: Object, default() { return {} } },
        reloadDebounce: { type: Number, default: 500 },
        noPaging: Boolean,
        noSearch: Boolean,
        exportable: Boolean,
        // support for rows selection
        selectable: Boolean,
        selectableId: { type: String, default: 'id' },
        pagingType: {
            type: String, 
            default: "full_numbers"
        },
        noSelect: {
            type: Boolean, default: false
        }
    },

    data() {
        return {
            selectedRows: [],
            table: null,
            processing: null,
            export: {
                excel: null,
                csv: null,
                tab: null
            }
        }
    },

    watch: {
        tableOptions() {
            this.init(true)
        }
    },

    methods: {
        reload: _.debounce(function(data) {
            this.table.ajax.data = this.addParamsToRequest(data || {});
            this.table.rows().deselect();
            this.table.ajax.reload();
        }, this.reloadDebounce),

        init(shouldDestroy = false) {

            var self = this;
            var options = $.extend({
                processing: false,
                destroy: shouldDestroy,
                select: !this.noSelect,
                serverSide: true,
                language:
                {
                    emptyTable: this.$t("DataTables.EmptyTable"),
                    info: this.$t("DataTables.Info"),
                    infoEmpty: this.$t("DataTables.InfoEmpty"),
                    infoFiltered: this.$t("DataTables.InfoFiltered"),
                    infoPostFix: this.$t("DataTables.InfoPostFix"),
                    thousands: this.$t("DataTables.InfoThousands"),
                    lengthMenu: this.$t("DataTables.LengthMenu"),
                    loadingRecords: "<div>" + this.$t("DataTables.LoadingRecords") + "</div>",
                    processing: "<div>" + this.$t("DataTables.Processing") + "</div>",
                    search: this.$t("DataTables.Search"),
                    searchPlaceholder: this.$t("DataTables.SearchPlaceholder"),
                    zeroRecords: this.$t("DataTables.ZeroRecords"),
                    paginate:
                    {
                        first: this.$t("DataTables.Paginate_First"),
                        last: this.$t("DataTables.Paginate_Last"),
                        next: this.$t("DataTables.Paginate_Next"),
                        previous: this.$t("DataTables.Paginate_Previous")
                    },
                    aria:
                    {
                        sortAscending: this.$t("DataTables.Aria_SortAscending"),
                        sortDescending:this.$t("DataTables.Aria_SortDescending")
                    }
                },
                orderMulti: this.multiorder,
                searchHighlight: true,
                pagingType: this.pagingType,
                lengthChange: false, // do not show page size selector
                pageLength: this.pageLength, // page size
                dom: "frtp",
                conditionalPaging: true,
                paging: !this.noPaging,
                searching: !this.noSearch
            }, this.tableOptions);

            if(this.hasTotalRow){
                options.createdRow = function(row, data, dataIndex){
                    if (dataIndex === 0){
                          $(row).addClass('total-row');
                    }
                }
            }

            if (this.selectable) {
                options.columns.unshift({
                    orderable: false,
                    className: 'checkbox-cell',
                    render(data, type, row) {
                        const id = row[self.selectableId];
                        const checkboxId = 'check-' + id;
                        return checkBox({ id, checkboxId });
                    },
                    responsivePriority: 1
                })
            }

            if(options.ajax != null) {
                options.ajax.dataSrc = (json) => {
                    if(json.data) {
                        if (json.data.length > 0 && json.totalRow) {
                            var totalRow = json.totalRow;
                            totalRow.DT_RowClass = "total-row";
                            json.data.unshift(totalRow);
                        }
                        return json.data
                    } else {
                        return json
                    }
                };
                        
                options.ajax.data = (d) => {
                    this.addParamsToRequest(d);

                    // reducing length of GET request URI
                    d.columns.forEach((column) => {
                        delete (column.orderable);
                        delete (column.search);
                        delete (column.data);
                        delete (column.searchable);
                    });
                    d._c = d.columns // aliasing columns arg name
                    delete d.columns

                    const requestUrl = this.table.ajax.url() + '?' + decodeURIComponent($.param(d));

                    if(this.exportable) {
                        this.export.excel = requestUrl + "&exportType=excel"
                        this.export.csv = requestUrl + "&exportType=csv"
                        this.export.tab = requestUrl + "&exportType=tab"
                    }
                };

                options.ajax.complete = (response) => {
                    self.$emit("totalRows", response.responseJSON.recordsTotal)
                    self.$emit("ajaxComplete", response.responseJSON);
                };
            }

            if(shouldDestroy) {
                this.table.destroy();
                $(this.$refs.table).empty();
            }

            this.table = $(this.$refs.table).DataTable(options);
            this.onTableInitComplete();
           
            this.table.on('select', (e, dt, type, indexes) => {
                self.rowsSelected(e, dt, type, indexes)
            });

            this.table.on('deselect', (e, dt, type, indexes) => {
                self.rowsDeselected(e, dt, type, indexes)
            });

            this.table.on('click', 'tbody td', ($el) => {
                const cell = self.table.cell($el.target);

                if (cell != null) {
                    const index = cell.index();

                    if (index != null && index.column > 0) {
                        var rowId = self.table.row($el.target).id();
                        var columns = self.table.settings().init().columns;

                        self.$emit('cell-clicked', columns[$el.target.cellIndex].name, rowId, cell.data());
                    }
                }
            });

            this.table.on('page', () => {
                self.$emit('page');
            });

            this.$emit('DataTableRef', this.table);
        },

        disableRow(rowIndex) {
            $(this.table.row(rowIndex).node()).addClass("disabled")
        },

        selectRowAndGetData(selectedItem) {
            this.table.rows().deselect();
            var rowIndex = selectedItem.parent().children().index(selectedItem);
            this.table.row(rowIndex).select();
            const rowData = this.table.rows({ selected: true }).data()[0];

            this.table.rows().deselect();

            return {
                rowData,
                rowIndex
            }
        },

        onTableInitComplete() {
            $(this.$refs.table).parents('.dataTables_wrapper').find('.dataTables_filter label').on('click', function(e) {
                if (e.target !== this)
                    return;
                if ($(this).hasClass("active")) {
                    $(this).removeClass("active");
                }
                else {
                    $(this).addClass("active");
                    $(this).children("input[type='search']").delay(200).queue(function() { $(this).focus(); $(this).dequeue(); });
                }
            });

            this.initContextMenu()
            this.initHeaderCheckBox()
            this.initProcessingBox()            
        },

        initProcessingBox() {
            this.table.on('processing', _.debounce(function(evnt, dt, show) {
                $(this).find(".dataTables_processing").css( 'display', show ? 'block' : 'none' );
            }, 250))

            $(this.$refs.table).prepend("<div class='dataTables_processing'><div>Processing...</div></div>");
        },

        initHeaderCheckBox() {
            if (!this.selectable) 
                return;

            var table = this.table;
            var firstHeader = table.column(0).header();
            $(firstHeader).html('<input class="double-checkbox" id="check-all" type="checkbox">' +
                '<label for="check-all">' +
                '<span class="tick"></span>' +
                '</label>');
            $('#check-all').change(function() {
                if (!this.checked) {
                    table.rows().deselect();
                    $(table.rows).find(".checkbox-filter").prop('checked', false);
                } else {
                    table.rows().select();
                    $(table.rows).find(".checkbox-filter").prop('checked', true);
                }
            });
        },

        initContextMenu() {
            if (this.contextMenuItems == null) return;
            var contextMenuOptions = {
                selector: "#" + this.$refs.table.attributes.id.value + " tbody tr",
                autoHide: false,
                build: ($trigger) => {
                    var selectedRow = this.selectRowAndGetData($trigger);

                    if (selectedRow.rowData == null) return false;

                    var items = this.contextMenuItems(selectedRow)
                    return { items: items };
                },
                trigger: 'left'
            };

            $.contextMenu(contextMenuOptions);
        },

        rowsSelected(e, dt, type, ar) {
            for (var i = 0; i < ar.length; i++) {
                var rowId = parseInt(dt.row(ar[i]).id());
                if (!_.includes(this.selectedRows, rowId)) {
                    this.selectedRows.push(rowId);
                }
            }

            this.$emit("selectedRowsChanged", this.selectedRows)
        },

        rowsDeselected(e, dt, type, ar) {
            for (var i = 0; i < ar.length; i++) {
                var rowId = dt.row(ar[i]).id();
                this.selectedRows = _.without(this.selectedRows, parseInt(rowId));
            }

            this.$emit("selectedRowsChanged", this.selectedRows)
        }
    },

    mounted() {
       this.init()
    }
}
</script>

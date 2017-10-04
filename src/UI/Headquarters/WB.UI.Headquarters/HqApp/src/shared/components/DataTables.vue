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
               v-bind:href="$store.state.exportUrls.excel">XLSX</a> {{$t("Pages.Or")}}
            <a target="_blank"
               v-bind:href="$store.state.exportUrls.csv">CSV</a> {{$t("Pages.Or")}}
            <a target="_blank"
               v-bind:href="$store.state.exportUrls.tab">TAB</a>
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
        wrapperClass: Object,
        responseProcessor: {
            type: Function,
            default(r) { return r; }
        },
        tableOptions: {
            type: Object,
            default() { return {}; }
        },
        contextMenuItems: {
            type: Function
        },
        authorizedUser: { type: Object, default() { return {} } },
        reloadDebounce: { type: Number, default: 500 },
        noPaging: Boolean,
        noSearch: Boolean,
        exportable: Boolean,

        // support for rows selection
        selectable: Boolean,
        selectableId: { type: String, default: 'id' }
    },

    data() {
        return {
            selectedRows: []
        }
    },

    methods: {
        reload: _.debounce(function(data) {
            this.table.ajax.data = this.addParamsToRequest(data || {});
            this.table.rows().deselect();
            this.table.ajax.reload();
        }, this.reloadDebounce),

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

            this.initContextMenu();
            this.initHeaderCheckBox();
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
        },

    },

    mounted() {
        var self = this;
        var options = $.extend({
            processing: true,
            select: true,
            serverSide: true,
            language:
            {
                "url": window.input.settings.config.dataTableTranslationsUrl,
            },
            searchHighlight: true,
            pagingType: "full_numbers",
            lengthChange: false, // do not show page size selector
            pageLength: 20, // page size
            dom: "frtp",
            conditionalPaging: true,
            paging: !this.noPaging,
            searching: !this.noSearch
        }, this.tableOptions);

        options.ajax.dataSrc = (json) => {
            if (json.data.length > 0 && json.totalRow) {
                var totalRow = json.totalRow;
                totalRow.DT_RowClass = "total-row";
                json.data.unshift(totalRow);
            }
            return json.data;
        };
        
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

        options.ajax.data = (d) => {
            this.addParamsToRequest(d);

            d.columns.forEach((column) => {
                delete (column.orderable);
                delete (column.search);
                delete (column.searchable);
            });

            var requestUrl = this.table.ajax.url() + '?' + decodeURIComponent($.param(d));

            this.$store.dispatch('setExportUrls', {
                excel: requestUrl + "&exportType=excel",
                csv: requestUrl + "&exportType=csv",
                tab: requestUrl + "&exportType=tab",
            });
        };

        options.ajax.complete = (response) => {
            self.$emit("totalRows", response.responseJSON.recordsTotal)
            self.$emit("ajaxComplete");
        };

        this.table = $(this.$refs.table).DataTable(options);
        this.table.on('init.dt', this.onTableInitComplete);

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
    }

}
</script>

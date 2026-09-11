// Shared AG Grid setup for the roster grids (TableRoster, MatrixRoster).
//
// Since AG Grid v33 every feature is provided by a module and a feature whose module is
// not registered is silently unavailable - that is how cell editing in tabular rosters
// was lost during a previous upgrade attempt. Register here every module the roster
// grids rely on and keep this list in sync with their grid options and column defs.
import {
    CellStyleModule,
    ClientSideRowModelModule,
    ColumnAutoSizeModule,
    CustomEditorModule,
    ModuleRegistry,
    RowAutoHeightModule,
    TextEditorModule,
    ValidationModule
} from 'ag-grid-community'

import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-quartz.css'

const modules = [
    ClientSideRowModelModule, // rowData based row model
    CellStyleModule, // colDef.cellStyle
    ColumnAutoSizeModule, // resizable columns
    CustomEditorModule, // custom vue cell editor components
    RowAutoHeightModule, // colDef.autoHeight
    TextEditorModule, // default editor of editable columns without a custom editor
]

if (import.meta.env.DEV) {
    // logs missing modules and invalid grid options into the browser console
    modules.push(ValidationModule)
}

ModuleRegistry.registerModules(modules)

// custom .ag-theme-customStyles styles are built on top of the legacy css themes,
// so the grids have to opt out of the theming api
export const agGridTheme = 'legacy'

export { AgGridVue } from 'ag-grid-vue3'

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

// Keeps the header of an auto height grid visible below the fixed navbar while the page
// scrolls through the grid. The header container is already sticky, but only within the
// grid viewport, which also owns the horizontal scroll and therefore can't drop its overflow.
// Pushing its sticky offset by the part of the grid hidden under the navbar moves it along
// with the page; the sticky containing block keeps it within the grid.
export function stickGridHeaderToPage(getGridElement) {
    let frame = null

    const update = () => {
        frame = null
        const header = getGridElement()?.querySelector('.ag-grid-pinned-top-rows')
        if (!header) return

        // grids with own vertical scroll (normal dom layout) keep the default sticky header
        if (!header.closest('.ag-grid-viewport')?.classList.contains('ag-layout-auto-height')) {
            header.style.top = ''
            return
        }

        const navbarHeight = document.querySelector('.navbar-fixed-top')?.offsetHeight || 0
        const hiddenPart = navbarHeight - header.parentElement.getBoundingClientRect().top
        header.style.top = hiddenPart > 0 ? `${hiddenPart}px` : ''
    }
    const scheduleUpdate = () => {
        if (frame == null) frame = requestAnimationFrame(update)
    }

    window.addEventListener('scroll', scheduleUpdate, { passive: true })
    window.addEventListener('resize', scheduleUpdate)
    scheduleUpdate()

    return () => {
        window.removeEventListener('scroll', scheduleUpdate)
        window.removeEventListener('resize', scheduleUpdate)
        if (frame != null) cancelAnimationFrame(frame)
    }
}

export { AgGridVue } from 'ag-grid-vue3'

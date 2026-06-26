let agGridStylesPromise

export function ensureAgGridStyles() {
    if (!agGridStylesPromise) {
        agGridStylesPromise = Promise.all([
            import('ag-grid-community/styles/ag-grid.css'),
            import('ag-grid-community/styles/ag-theme-quartz.css'),
        ])
    }

    return agGridStylesPromise
}
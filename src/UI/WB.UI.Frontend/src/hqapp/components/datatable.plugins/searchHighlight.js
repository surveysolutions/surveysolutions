/**
 * SearchHighlight for DataTables
 * Based on the original SpryMedia plugin, rewritten to use vanilla JS
 * instead of the jquery-highlight dependency.
 *
 * Search highlighting in DataTables can be enabled by:
 *
 * * Adding the class `searchHighlight` to the HTML table
 * * Setting the `searchHighlight` parameter in the DataTables initialisation to
 *   be true
 * * Setting the `searchHighlight` parameter to be true in the DataTables
 *   defaults (thus causing all tables to have this feature) - i.e.
 *   `$.fn.dataTable.defaults.searchHighlight = true`.
 */

(function (window, document, $) {

    function highlightText(container, terms, className) {
        unhighlightText(container, className)
        var filtered = terms.filter(function (t) { return t.length > 0 })
        if (!filtered.length) return

        var escaped = filtered.map(function (t) {
            return t.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
        })
        var regex = new RegExp('(' + escaped.join('|') + ')', 'gi')

        var walker = document.createTreeWalker(container, NodeFilter.SHOW_TEXT)
        var nodes = []
        while (walker.nextNode()) nodes.push(walker.currentNode)

        for (var i = 0; i < nodes.length; i++) {
            var node = nodes[i]
            if (!regex.test(node.nodeValue)) { regex.lastIndex = 0; continue }
            regex.lastIndex = 0

            var frag = document.createDocumentFragment()
            var last = 0, match
            while ((match = regex.exec(node.nodeValue))) {
                if (match.index > last) {
                    frag.appendChild(document.createTextNode(node.nodeValue.slice(last, match.index)))
                }
                var mark = document.createElement('span')
                mark.className = className || 'highlight'
                mark.textContent = match[0]
                frag.appendChild(mark)
                last = regex.lastIndex
            }
            if (last < node.nodeValue.length) {
                frag.appendChild(document.createTextNode(node.nodeValue.slice(last)))
            }
            node.parentNode.replaceChild(frag, node)
        }
    }

    function unhighlightText(container, className) {
        var cls = className || 'highlight'
        var spans = container.querySelectorAll('span.' + cls)
        for (var i = 0; i < spans.length; i++) {
            var el = spans[i]
            el.replaceWith(document.createTextNode(el.textContent))
        }
        container.normalize()
    }

    function highlight(body, table) {
        var globalTerms = $.trim(table.search()).split(/\s+/)
        var hasAppliedRows = table.rows({ filter: 'applied' }).data().length

        if (hasAppliedRows) {
            table.columns().every(function () {
                var column = this
                var columnNodes = column.nodes().flatten().to$()
                columnNodes.each(function () {
                    highlightText(this, $.trim(column.search()).split(/\s+/), 'column_highlight')
                })
            })
            highlightText(body, globalTerms)
        } else {
            unhighlightText(body)
            unhighlightText(body, 'column_highlight')
        }
    }

    // Listen for DataTables initialisations
    $(document).on('init.dt.dth', function (e, settings) {
        if (e.namespace !== 'dt') {
            return
        }

        var table = new $.fn.dataTable.Api(settings)
        var body = table.table().body()

        if (
            $(table.table().node()).hasClass('searchHighlight') || // table has class
            settings.oInit.searchHighlight || // option specified
            $.fn.dataTable.defaults.searchHighlight                    // default set
        ) {
            table
                .on('draw.dt.dth column-visibility.dt.dth column-reorder.dt.dth', function () {
                    highlight(body, table)
                })
                .on('destroy', function () {
                    table.off('draw.dt.dth column-visibility.dt.dth column-reorder.dt.dth')
                })

            if (table.search()) {
                highlight(body, table)
            }
        }
    })

})(window, document, window.jQuery)

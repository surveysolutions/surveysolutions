// Scans .vue templates for static text/attributes that look like user-facing
// strings but aren't routed through $t(...), to help find localization gaps.
//
// Usage: node tools/findHardcodedLiterals.js [globPattern]
// Example: node tools/findHardcodedLiterals.js "src/hqapp/**/*.vue"
import fg from 'fast-glob'
import fs from 'fs'
import { parse as parseSFC } from '@vue/compiler-sfc'
import { parse as parseTemplate, NodeTypes } from '@vue/compiler-dom'

// Attributes known to hold user-facing text; flagged with high confidence.
const TEXT_ATTRS = new Set([
    'placeholder', 'title', 'alt', 'label',
    'aria-label', 'aria-placeholder', 'aria-valuetext', 'aria-description',
    'summary',
])

// Attributes that are structural/technical and never hold translatable text.
const IGNORED_ATTRS = new Set([
    'class', 'style', 'id', 'ref', 'key', 'slot', 'name', 'type', 'for',
    'href', 'src', 'rel', 'target', 'role', 'tabindex', 'method', 'action',
    'autocomplete', 'maxlength', 'minlength', 'min', 'max', 'step', 'pattern',
    'accept', 'colspan', 'rowspan', 'xmlns', 'viewBox', 'd', 'points', 'fill',
    'stroke', 'width', 'height', 'x', 'y', 'cx', 'cy', 'r', 'transform',
    'preserveAspectRatio', 'stroke-width', 'stroke-linecap', 'stroke-linejoin',
    'data-bs-toggle', 'data-bs-target', 'novalidate', 'autocapitalize',
    'spellcheck', 'draggable', 'contenteditable',
])

// A string "looks like" translatable text if it has letters and either
// contains a space (a phrase) or starts with an uppercase letter (a label).
function looksLikeText(value) {
    const trimmed = value.trim()
    if (trimmed.length < 3) return false
    if (!/[A-Za-z]{2,}/.test(trimmed)) return false
    if (/^[A-Za-z0-9_-]+$/.test(trimmed) && !/[A-Z]/.test(trimmed[0])) return false
    return / /.test(trimmed) || /^[A-Z]/.test(trimmed)
}

function lineOf(loc, offset) {
    return loc.start.line + offset
}

function walk(node, ctx, results) {
    if (node.type === NodeTypes.TEXT) {
        const content = node.content
        if (looksLikeText(content)) {
            results.push({
                kind: 'text',
                line: lineOf(node.loc, ctx.lineOffset),
                snippet: content.trim().slice(0, 80),
            })
        }
        return
    }

    if (node.type === NodeTypes.ELEMENT) {
        for (const prop of node.props || []) {
            if (prop.type === NodeTypes.ATTRIBUTE && prop.value) {
                const attrName = prop.name
                if (IGNORED_ATTRS.has(attrName)) continue
                if (attrName.startsWith('on') || attrName.startsWith('data-')) continue
                const value = prop.value.content
                if (!looksLikeText(value)) continue
                results.push({
                    kind: TEXT_ATTRS.has(attrName) ? `attr:${attrName}` : `attr?:${attrName}`,
                    line: lineOf(prop.loc, ctx.lineOffset),
                    snippet: value.trim().slice(0, 80),
                })
            }
        }
    }

    for (const child of node.children || []) {
        walk(child, ctx, results)
    }
}

function scanFile(filePath) {
    const source = fs.readFileSync(filePath, 'utf-8')
    const { descriptor } = parseSFC(source, { filename: filePath })
    if (!descriptor.template) return []

    const ast = parseTemplate(descriptor.template.content, {
        onError: () => { }, // template may use syntax the standalone parser can't fully resolve; ignore
    })

    const lineOffset = descriptor.template.loc.start.line - 1
    const results = []
    walk(ast, { lineOffset }, results)
    return results
}

function main() {
    const pattern = process.argv[2] || 'src/**/*.vue'
    const files = fg.sync(pattern, { onlyFiles: true, cwd: process.cwd() })

    let total = 0
    for (const file of files.sort()) {
        let matches
        try {
            matches = scanFile(file)
        } catch (err) {
            console.warn(`skip ${file}: ${err.message}`)
            continue
        }
        if (matches.length === 0) continue

        total += matches.length
        console.log(`\n${file}`)
        for (const m of matches) {
            console.log(`  L${m.line} [${m.kind}] ${m.snippet}`)
        }
    }

    console.log(`\nTotal candidate literals: ${total} across ${files.length} files scanned.`)
}

main()

import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import { test } from 'node:test'
import postcss from 'postcss'
import * as sass from 'sass'
import { compileStyleAsync, parse } from 'vue/compiler-sfc'
import { createSSRApp } from 'vue'
import { renderToString } from 'vue/server-renderer'
import webInterviewRtl from '../../tools/postcss-webinterview-rtl.js'

const frontend = new URL('../../', import.meta.url)
const ltr = ':where(html:not(.web-interview-page[dir="rtl"]))'
const rtl = ':where(html.web-interview-page[dir="rtl"])'

async function transform(css, file = 'src/assets/css/markup.scss') {
    return postcss([webInterviewRtl()]).process(css, {
        from: fileURLToPath(new URL(file, frontend)),
    })
}

function declarations(root, selector, property) {
    const values = []
    root.walkRules(rule => {
        if (!rule.selectors.includes(selector)) return
        rule.walkDecls(property, declaration => values.push(declaration.value))
    })
    return values
}

async function componentStyles(name, folder = 'webinterview/components') {
    const filename = fileURLToPath(new URL(`src/${folder}/${name}.vue`, frontend))
    const { descriptor, errors } = parse(readFileSync(filename, 'utf8'), { filename })
    assert.deepEqual(errors, [])
    const style = descriptor.styles[0]
    const compiled = await compileStyleAsync({
        filename,
        source: style.content,
        id: 'data-v-rtl-test',
        scoped: style.scoped,
        preprocessLang: style.lang,
        preprocessOptions: { logger: sass.Logger.silent },
    })
    assert.deepEqual(compiled.errors, [])
    return postcss.parse(compiled.code)
}

test('completion component uses logical alignment, spacing and stripe corners in scoped styles', async () => {
    const root = await componentStyles('Complete')
    for (const selector of [
        '.tab-item .tab-title',
        '.tab-item .tab-count',
        '.tab-content-item .item-title',
        '.tab-content-item .item-error',
        '.tab-content-item .item-comment',
    ]) {
        assert.deepEqual(declarations(root, `${selector}[data-v-rtl-test]`, 'text-align'), ['start'])
    }
    const stripe = '.tab-content .tab-content-item[data-v-rtl-test]::before'
    assert.deepEqual(declarations(root, stripe, 'inset-inline-start'), ['0'])
    assert.deepEqual(declarations(root, stripe, 'left'), [])
    assert.deepEqual(declarations(root, stripe, 'border-radius'), ['0'])
    assert.deepEqual(declarations(root, stripe, 'border-start-start-radius'), ['4px'])
    assert.deepEqual(declarations(root, stripe, 'border-end-start-radius'), ['4px'])
    assert.deepEqual(declarations(root, '.tab-content-item[data-v-rtl-test]', 'padding-inline-start'), ['20px'])
    assert.deepEqual(declarations(root, '.and-more[data-v-rtl-test]', 'padding-inline-start'), ['19px'])
})

test('reconnect button uses an automatic margin on the inline start side', async () => {
    const root = await componentStyles('ReconnectingBanner')
    const selector = '.reconnecting-banner__reload[data-v-rtl-test]'
    assert.deepEqual(declarations(root, selector, 'margin-inline-start'), ['auto'])
    assert.deepEqual(declarations(root, selector, 'margin-left'), [])
    assert.deepEqual(declarations(root, selector, 'margin-right'), [])
})

test('generates scoped RTL and preserves unmarked HQ and LTR rules without adding specificity', async () => {
    const { root } = await transform('.question { margin-left: 12px; padding: 1px 2px 3px 4px; text-align: left }')
    assert.deepEqual(declarations(root, `${ltr} .question`, 'margin-left'), ['12px'])
    assert.deepEqual(declarations(root, `${rtl} .question`, 'margin-right'), ['12px'])
    assert.deepEqual(declarations(root, `${ltr} .question`, 'padding'), ['1px 2px 3px 4px'])
    assert.deepEqual(declarations(root, `${rtl} .question`, 'padding'), ['1px 4px 3px 2px'])
    assert.deepEqual(declarations(root, `${rtl} .question`, 'text-align'), ['right'])
})

test('prefixes html itself rather than requiring a nested html element', async () => {
    const { root } = await transform('html { padding-left: 12px }')
    assert.deepEqual(declarations(root, `html${rtl}`, 'padding-right'), ['12px'])
})

test('preserves reset order and media queries', async () => {
    const { root } = await transform(`
        .question { padding-left: 20px }
        .reset { padding: 0 }
        @media (max-width: 768px) { .question { padding-left: 5px } }
    `)
    assert.deepEqual(declarations(root, '.reset', 'padding'), ['0'])
    const media = root.nodes.find(node => node.type === 'atrule')
    assert.equal(media.params, '(max-width: 768px)')
    assert.deepEqual(declarations(media, `${rtl} .question`, 'padding-right'), ['5px'])
})

test('preserves explicitly marked sprite coordinates without freezing other declarations', async () => {
    const css = sass.compileString(`
        @import "variables";
        @import "mixins";
        .tick { left: 2px; background-image: url("questionnaire-sprite.png"); @include sprite-position(0 0); }
        .checked { @include sprite-position(-78px 2px); }
        .flag { @include sprite-position(1px 0); }
    `, {
        loadPaths: [fileURLToPath(new URL('src/assets/css', frontend))],
        logger: sass.Logger.silent,
    }).css
    const { root } = await transform(css)
    assert.deepEqual(declarations(root, '.tick', 'background-position'), ['0 0'])
    assert.deepEqual(declarations(root, '.checked', 'background-position'), ['-78px 2px'])
    assert.deepEqual(declarations(root, '.flag', 'background-position'), ['1px 0'])
    assert.deepEqual(declarations(root, '.tick', 'background-image'), ['url("questionnaire-sprite.png")'])
    assert.deepEqual(declarations(root, `${rtl} .tick`, 'right'), ['2px'])
})

test('does not process native RTL vendor styles or unrelated entry points', async () => {
    const css = '.ag-rtl { left: 20px; direction: rtl }'
    for (const file of [
        'node_modules/ag-grid-community/styles/ag-grid.css',
        'src/assets/css/markup-specific.scss',
        'src/shared/components/Bootstrap5Popover.vue',
    ]) {
        assert.equal((await transform(css, file)).css, css)
    }
})

test('explicit RTL exceptions are not mirrored a second time', async () => {
    const { root } = await transform(`
        /* rtl:begin:ignore */
        html.web-interview-page[dir="rtl"] input { direction: ltr; text-align: right }
        /* rtl:end:ignore */
    `)
    assert.deepEqual(declarations(root, 'html.web-interview-page[dir="rtl"] input', 'direction'), ['ltr'])
    assert.deepEqual(declarations(root, 'html.web-interview-page[dir="rtl"] input', 'text-align'), ['right'])
})

test('actual styles mirror desktop and mobile sidebar positions and retain numeric exceptions', async () => {
    for (const name of ['markup', 'markup-web-interview']) {
        const file = `src/assets/css/${name}.scss`
        const compiled = sass.compile(fileURLToPath(new URL(file, frontend)), {
            logger: sass.Logger.silent,
        })
        const { root } = await transform(compiled.css, file)
        if (name === 'markup-web-interview') {
            assert.ok(declarations(root, `${rtl} .content`, 'right').includes('12px'))
            assert.ok(declarations(root, `${rtl} .web-interview .content`, 'right').some(value => value.startsWith('-')))
            assert.ok(declarations(root, `${rtl} .web-interview .content`, 'left').length === 0)
            assert.ok(declarations(root, `${ltr} .content`, 'left').includes('12px'))
            for (const state of ['has-error', 'has-warnings']) {
                const selector = `.table-view .ag-theme-customStyles .cell-unit.${state}`
                assert.deepEqual(declarations(root, `${rtl} ${selector}`, 'background-position'), ['100% 0'])
                assert.deepEqual(declarations(root, `${ltr} ${selector}`, 'background-position'), ['0 0'])
            }
            const trigger = '.table-view .ag-theme-customStyles.roster-matrix .ag-cell .tooltip-wrapper'
            assert.deepEqual(declarations(root, `${rtl} ${trigger}`, 'right'), ['0'])
            assert.deepEqual(declarations(root, `${ltr} ${trigger}`, 'left'), ['0'])
        } else {
            const spritePositions = []
            postcss.parse(compiled.css).walkDecls('background-position', declaration => {
                if (declaration.prev()?.type === 'comment' && declaration.prev().text === 'rtl:ignore') {
                    spritePositions.push(declaration)
                }
            })
            assert.ok(spritePositions.length >= 50, 'all existing sprite positions must be protected')
            for (const declaration of spritePositions) {
                for (const selector of declaration.parent.selectors) {
                    assert.ok(declarations(root, selector, declaration.prop).includes(declaration.value),
                        `sprite position changed for ${selector}`)
                    assert.deepEqual(declarations(root, `${rtl} ${selector}`, declaration.prop), [])
                }
            }
            assert.deepEqual(declarations(root, 'html.web-interview-page[dir=rtl] input[inputmode=decimal]', 'direction'), ['ltr'])
            assert.deepEqual(declarations(root, 'html.web-interview-page[dir=rtl] .flatpickr-calendar', 'direction'), ['ltr'])
            assert.deepEqual(declarations(root, 'html.web-interview-page[dir=rtl] .btn-back::before', 'transform'), ['scaleX(-1)'])
            assert.deepEqual(declarations(root, 'html.web-interview-page[dir=rtl] .table-view .ag-theme-customStyles .ag-grid-pinned-right-cells .ag-header-cell', 'border-left'), ['1px solid rgba(0, 0, 0, 0.3)'])
        }
    }
})

test('saved numeric roster answers are isolated in LTR while text answers inherit direction', async () => {
    const filename = fileURLToPath(new URL('src/webinterview/components/questions/TableRoster.ViewAnswer.vue', frontend))
    const { descriptor, errors } = parse(readFileSync(filename, 'utf8'), { filename })
    assert.deepEqual(errors, [])
    const { default: component } = await import(`data:text/javascript;base64,${Buffer.from(descriptor.script.content).toString('base64')}`)
    for (const type of ['Integer', 'Double', 'Text']) {
        for (const useFormatting of [false, true]) {
            const answer = type === 'Text' ? 'مرحبا ABC' : type === 'Integer' ? -1234 : -1234.5
            const app = createSSRApp({
                ...component,
                template: descriptor.template.content,
                props: ['params'],
            }, { params: { value: { identity: 'q1', type } } })
            app.config.globalProperties.$store = {
                state: { webinterview: {
                    entityDetails: { q1: {
                        answer,
                        useFormatting,
                        validity: { isValid: true, messages: [], warnings: [] },
                    } },
                    fetch: { state: {} },
                } },
            }
            app.component('popover', { template: '<div><slot /></div>' })
            app.component('wb-progress', { template: '<span />' })
            app.directive('dateTimeFormatting', {})
            app.directive('dompurify-html', {})
            const html = await renderToString(app)
            if (type === 'Text') {
                assert.doesNotMatch(html, /<bdi|dir="ltr"/)
                assert.ok(html.includes(answer))
            } else {
                const expected = useFormatting ? answer.toLocaleString(undefined, {
                    style: 'decimal', maximumFractionDigits: 15, minimumFractionDigits: 0,
                }) : String(answer)
                assert.ok(html.includes(`<bdi dir="ltr">${expected}</bdi>`), html)
            }
        }
    }
})

test('HQ review mirrors filters, navigation and flags while preserving readonly sprite coordinates', async () => {
    const file = 'src/assets/css/markup-interview-review.scss'
    const compiled = sass.compile(fileURLToPath(new URL(file, frontend)), { logger: sass.Logger.silent })
    const { root } = await transform(compiled.css, file)
    for (const [selector, positions] of [
        ['.web-interview-for-supervisor .content', ['342px', '282px', '-330px']],
        ['.web-interview-for-supervisor .filters', ['-282px']],
        ['.web-interview-for-supervisor.show-filters .filters', ['0']],
        ['.web-interview-for-supervisor.show-content .content', ['12px']],
        ['.web-interview-for-supervisor.fullscreen-hidden-content .content', ['-330px']],
        ['.web-interview-for-supervisor.fullscreen-hidden-filters .filters', ['-282px']],
        ['.filters-results.active', ['342px']],
        ['.web-interview-for-supervisor .filters-results.active', ['282px']],
    ]) {
        for (const position of positions) {
            assert.ok(declarations(root, `${ltr} ${selector}`, 'left').includes(position), `${selector}: ${position}`)
            assert.ok(declarations(root, `${rtl} ${selector}`, 'right').includes(position), `${selector}: ${position}`)
        }
        assert.deepEqual(declarations(root, `${rtl} ${selector}`, 'left'), [])
    }
    assert.deepEqual(declarations(root, `${rtl} .question.with-flag`, 'background-position'), ['right top'])
    assert.deepEqual(declarations(root, `${ltr} .question.with-flag`, 'background-position'), ['left top'])
    let spriteCount = 0
    postcss.parse(compiled.css).walkDecls('background-position', declaration => {
        if (declaration.prev()?.type !== 'comment' || declaration.prev().text !== 'rtl:ignore') return
        spriteCount++
        for (const selector of declaration.parent.selectors) {
            assert.ok(declarations(root, selector, declaration.prop).includes(declaration.value))
            assert.deepEqual(declarations(root, `${rtl} ${selector}`, declaration.prop), [])
        }
    })
    assert.equal(spriteCount, 6)
})

test('HQ review audio panel uses logical edges and text alignment', async () => {
    const root = await componentStyles('AudioAuditPanel', 'hqapp/Views/HQ/Interview')
    const panel = '.audio-audit-panel[data-v-rtl-test]'
    assert.deepEqual(declarations(root, panel, 'inset-inline-end'), ['0'])
    assert.deepEqual(declarations(root, panel, 'inset-inline-start'), ['0'])
    assert.deepEqual(declarations(root, panel, 'right'), [])
    assert.deepEqual(declarations(root, panel, 'border-inline-start'), ['1px solid #d9d9d9', '0'])
    assert.deepEqual(declarations(root, '.playlist-item[data-v-rtl-test]', 'text-align'), ['start'])
})

test('only HQ review opts into culture-based RTL in the shared layout before scripts load', () => {
    const view = readFileSync(new URL('../WB.UI.Headquarters.Core/Views/Interview/Review.cshtml', frontend), 'utf8')
    assert.match(view, /ViewData\["WebInterviewPage"\] = true;/)
    const layout = readFileSync(new URL('../WB.UI.Headquarters.Core/Views/Shared/_Layout.Template.cshtml', frontend), 'utf8')
    assert.match(layout, /var isWebInterviewPage = ViewData\["WebInterviewPage"\] is true;/)
    assert.match(layout, /dir="@\(isWebInterviewPage \? \(CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft \? "rtl" : "ltr"\) : null\)"/)
    assert.match(layout, /class="@\(isWebInterviewPage \? "web-interview-page" : null\)"/)
    assert.ok(layout.indexOf('dir=') < layout.indexOf('<script'))
})

test('all interview page templates select direction from UI culture before the app mounts', () => {
    for (const file of [
        '../WB.UI.Headquarters.Core/Views/WebInterview/Index.Template.cshtml',
        '../WB.UI.Headquarters.Core/Views/WebInterview/_WebInterviewLayout.Template.cshtml',
        '../WB.UI.WebTester/Views/Shared/_Layout.Template.cshtml',
    ]) {
        const source = readFileSync(new URL(file, frontend), 'utf8')
        assert.match(source, /<html lang="@System\.Globalization\.CultureInfo\.CurrentUICulture\.Name"/)
        assert.match(source, /dir="@\(System\.Globalization\.CultureInfo\.CurrentUICulture\.TextInfo\.IsRightToLeft \? "rtl" : "ltr"\)"/)
        assert.match(source, /class="web-interview-page"/)
    }
})

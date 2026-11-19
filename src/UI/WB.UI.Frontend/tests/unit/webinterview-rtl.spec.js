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
const ltr = ':where(html:not([dir="rtl"]))'
const rtl = ':where(html[dir="rtl"])'

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

test('generates RTL and preserves LTR and pages without direction without adding specificity', async () => {
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
        'src/assets/css/landing-page.scss',
        'src/shared/components/Bootstrap5Popover.vue',
    ]) {
        assert.equal((await transform(css, file)).css, css)
    }
})

test('explicit RTL exceptions are not mirrored a second time', async () => {
    const { root } = await transform(`
        /* rtl:begin:ignore */
        html[dir="rtl"] input { direction: ltr; text-align: right }
        /* rtl:end:ignore */
    `)
    assert.deepEqual(declarations(root, 'html[dir="rtl"] input', 'direction'), ['ltr'])
    assert.deepEqual(declarations(root, 'html[dir="rtl"] input', 'text-align'), ['right'])
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
            assert.deepEqual(declarations(root, 'html[dir=rtl] input[inputmode=decimal]', 'direction'), ['ltr'])
            assert.deepEqual(declarations(root, 'html[dir=rtl] input[type=number]', 'direction'), ['ltr'])
            assert.deepEqual(declarations(root, 'html[dir=rtl] code', 'direction'), ['ltr'])
            assert.deepEqual(declarations(root, 'html[dir=rtl] .countDown', 'direction'), ['ltr'])
            assert.deepEqual(declarations(root, 'html[dir=rtl] .flatpickr-input', 'direction'), ['ltr'])
            assert.ok(declarations(root, `${rtl} .navbar-header`, 'float').includes('right'))
            assert.ok(declarations(root, `${ltr} .navbar-header`, 'float').includes('left'))
            assert.deepEqual(declarations(root, 'html[dir=rtl] .flatpickr-calendar', 'direction'), ['ltr'])
            const cells = ':where(html[dir=rtl]) :where(td, th, .btn-input > .dropdown-toggle, .dropdown-menu > li > a)'
            assert.deepEqual(declarations(root, cells, 'unicode-bidi'), ['plaintext'])
            assert.deepEqual(declarations(root, cells, 'text-align'), ['-webkit-match-parent', 'match-parent'])
            let textFields = null
            root.walkRules(rule => {
                if (rule.selector.startsWith(':where(html[dir=rtl]) :where(input:not([type])')) textFields = rule
            })
            assert.ok(textFields, 'free-text fields must resolve direction from content')
            assert.match(textFields.selector, /input\[type=password\]/)
            assert.match(textFields.selector, /:not\(\[inputmode=numeric\], \[inputmode=decimal\], \.flatpickr-input, \.numeric-question input, \.time-question input\)/)
            assert.deepEqual(declarations(root, textFields.selector, 'unicode-bidi'), ['plaintext'])
            assert.deepEqual(declarations(root, textFields.selector, 'text-align'), ['right'])
            let questionnaireText = null
            root.walkRules(rule => {
                if (rule.selector.includes('.question-editor .options-group .field > label')) questionnaireText = rule
            })
            assert.ok(questionnaireText, 'questionnaire text must resolve direction from its own content')
            for (const selector of ['.question-editor h5 > span', '.question-editor .options-group .field > span',
                '.information-block.instruction > div', '.readonly-answer-group .container-info p > b',
                '.unit-title h3', '.structured-content .panel-title > a', '.action-container .btn',
                '.overview-item .item-content > h4', '.overview-item .answer']) {
                assert.ok(questionnaireText.selector.includes(selector), selector)
            }
            assert.ok(questionnaireText.selector.startsWith(':where(html[dir=rtl]) :where('))
            assert.deepEqual(declarations(root, questionnaireText.selector, 'unicode-bidi'), ['plaintext'])
            assert.deepEqual(declarations(root, questionnaireText.selector, 'text-align'), ['-webkit-match-parent', 'match-parent'])
            assert.deepEqual(declarations(root, 'html[dir=rtl] .btn-back::before', 'transform'), ['scaleX(-1)'])
            assert.deepEqual(declarations(root, '.modal-dialog', 'text-align'), ['start'])
            assert.deepEqual(declarations(root, '.modal-content .modal-footer', 'text-align'), ['start'])
            assert.deepEqual(declarations(root, 'html[dir=rtl] .table-view .ag-theme-customStyles .ag-grid-pinned-right-cells .ag-header-cell', 'border-left'), ['1px solid rgba(0, 0, 0, 0.3)'])
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

test('all HQ layouts select direction from UI culture without requiring a review-page flag', () => {
    for (const file of [
        'Shared/_Layout.Template.cshtml',
        'Shared/_Logon.Template.cshtml',
        'Shared/_EmptyLayout.Template.cshtml',
        'Shared/_FinishInstallation.Template.cshtml',
        'Shared/_WebInterview.Template.cshtml',
        'UnderConstruction/Index.Template.cshtml',
        'Error/_LayoutForErrorPage.cshtml',
        'Maps/MapPreview.cshtml',
        'Interview/InterviewAreaFrame.cshtml',
    ]) {
        const layout = readFileSync(new URL(`../WB.UI.Headquarters.Core/Views/${file}`, frontend), 'utf8')
        assert.match(layout, /<html lang="@(System\.Globalization\.)?CultureInfo\.CurrentUICulture\.Name"/)
        assert.match(layout, /dir="@\((System\.Globalization\.)?CultureInfo\.CurrentUICulture\.TextInfo\.IsRightToLeft \? "rtl" : "ltr"\)"/)
        assert.doesNotMatch(layout, /WebInterviewPage/)
        if (layout.includes('<script')) assert.ok(layout.indexOf('dir=') < layout.indexOf('<script'))
    }
})

test('HQ-specific styles mirror export and action panels without changing sprite pixels', async () => {
    const file = 'src/assets/css/markup-specific.scss'
    const compiled = sass.compile(fileURLToPath(new URL(file, frontend)), { logger: sass.Logger.silent })
    const { root } = await transform(compiled.css, file)
    assert.deepEqual(declarations(root, `${rtl} .export-types`, 'padding'), ['0 30px 0 0', '0 30px 0 10px'])
    assert.deepEqual(declarations(root, `${ltr} .export-types`, 'padding'), ['0 0 0 30px', '0 10px 0 30px'])
    assert.deepEqual(declarations(root, `${rtl} .format-data`, 'padding-right'), ['80px'])
    assert.ok(declarations(root, `${rtl} .panel-table label .tick`, 'margin-left').includes('20px'))
    let spriteCount = 0
    postcss.parse(compiled.css).walkDecls('background-position', declaration => {
        if (declaration.prev()?.type !== 'comment' || declaration.prev().text !== 'rtl:ignore') return
        spriteCount++
        for (const selector of declaration.parent.selectors) {
            assert.ok(declarations(root, selector, declaration.prop).includes(declaration.value), selector)
            assert.deepEqual(declarations(root, `${rtl} ${selector}`, declaration.prop), [])
        }
    })
    assert.equal(spriteCount, 26)
})

test('HQ scoped styles follow direction without mirroring pixel-based popover positioning', async () => {
    const popover = await componentStyles('Bootstrap5Popover', 'shared/components')
    assert.deepEqual(declarations(popover, '.bs5-popover[data-v-rtl-test]', 'text-align'), ['start'])
    assert.deepEqual(declarations(popover, '.bs5-popover[data-v-rtl-test]', 'left'), ['0'])
    const editor = await componentStyles('MdEditor', 'hqapp/components')
    const dropdown = '.editor-toolbar .dropdown-menu[data-v-rtl-test]'
    assert.deepEqual(declarations(editor, dropdown, 'inset-inline-start'), ['0'])
    assert.deepEqual(declarations(editor, dropdown, 'inset-inline-end'), ['auto'])
})

test('chart legends and tooltips follow page direction without reversing axes', () => {
    const filename = fileURLToPath(new URL('src/hqapp/Views/HQ/Reports/CumulativeChart.vue', frontend))
    const { descriptor } = parse(readFileSync(filename, 'utf8'), { filename })
    const script = descriptor.script.content
    assert.match(script, /const isRtl = document\.documentElement\.dir === 'rtl'/)
    assert.equal(script.match(/rtl: isRtl/g)?.length, 2)
    assert.equal(script.match(/textDirection: isRtl \? 'rtl' : 'ltr'/g)?.length, 2)
    assert.doesNotMatch(script, /\breverse\s*:/)
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

test('interview keys and questionnaire versions are isolated next to translated labels', () => {
    for (const [file, pattern] of [
        ['hqapp/Views/HQ/Interview/DetailsInfo.vue', /<bdi dir="ltr">\{\{ \$config\.model\.key \}\}<\/bdi>/],
        ['hqapp/Views/HQ/Interview/DetailsInfo.vue', /<bdi>\[ver\.\{\{ this\.\$config\.model\.questionnaireVersion \}\}\]<\/bdi>\s*<bdi>\{\{ this\.\$config\.model\.questionnaireTitle \}\}<\/bdi>/],
        ['hqapp/Views/HQ/Interview/DetailsInfo.vue', /<a dir="ltr"\s+:href="this\.\$config\.model\.assignmentDetailsUrl">#/],
        ['hqapp/Views/HQ/Interview/StatusesHistory.vue', /<bdi dir="ltr"\s+v-text="\$config\.model\.key"><\/bdi>/],
        ['hqapp/components/GoogleMap.vue', /<bdi dir="ltr"\s+v-text="selectedTooltip\.interviewKey"><\/bdi>/],
        ['hqapp/Views/HQ/Reports/MapReport.vue', /<bdi dir="ltr"\s+v-text="selectedTooltip\.interviewKey"><\/bdi>/],
    ]) {
        assert.match(readFileSync(new URL(`src/${file}`, frontend), 'utf8'), pattern, file)
    }
})

test('Arabic DataTables strings keep the placeholders DataTables substitutes', () => {
    const resources = new URL('../WB.UI.Headquarters.Core/Resources/', frontend)
    const values = file => Object.fromEntries([...readFileSync(new URL(file, resources), 'utf8')
        .matchAll(/<data name="([^"]+)"[^>]*>\s*<value>([^<]*)<\/value>/g)].map(match => [match[1], match[2]]))
    const english = values('DataTables.resx')
    const arabic = values('DataTables.ar.resx')
    const placeholders = text => (text.match(/_[A-Z]+_/g) ?? []).sort()
    for (const [name, text] of Object.entries(english)) {
        if (name in arabic) assert.deepEqual(placeholders(arabic[name]), placeholders(text), name)
    }
})

test('email preview direction follows the first letter of authored text like the server', async () => {
    const { emailDirection } = await import('../../src/shared/emailDirection.js')
    const originalDomParser = globalThis.DOMParser
    globalThis.DOMParser = class {
        parseFromString(text) {
            return {
                body: {
                    textContent: text.replace(/&(lt|gt|amp);/g, entity => ({
                        '&lt;': '<',
                        '&gt;': '>',
                        '&amp;': '&',
                    })[entity]),
                },
            }
        }
    }
    const cases = [
        [{ subject: 'دعوة للمشاركة', message: 'مرحبا' }, 'rtl'],
        [{ subject: '%SURVEYNAME%', message: 'مرحبا بكم في %SURVEYNAME%' }, 'rtl'],
        [{ subject: '2026: دعوة', message: '' }, 'rtl'],
        [{ subject: 'Invitation', message: 'مرحبا' }, 'ltr'],
        [{ subject: '', message: '<b>Hello</b> مرحبا' }, 'ltr'],
        [{ subject: '', message: '&lt;مرحبا&gt;' }, 'rtl'],
        [{ subject: '', message: '&amp;lt;مرحبا&amp;gt;' }, 'ltr'],
        [{ subject: '%QUESTIONNAIRE%', message: '%SURVEYLINK%', linkText: 'ابدأ' }, 'rtl'],
        [{ subject: '%SURVEYNAME%', message: '%SURVEYLINK%' }, 'ltr'],
        [{ subject: '٢٠٢٦ Survey' }, 'ltr'],
        [null, 'ltr'],
    ]
    try {
        for (const [template, direction] of cases) {
            assert.equal(emailDirection(template), direction, JSON.stringify(template))
        }
    } finally {
        if (originalDomParser === undefined) delete globalThis.DOMParser
        else globalThis.DOMParser = originalDomParser
    }
    const settings = readFileSync(new URL('src/hqapp/Views/HQ/WebInterviewSetup/Settings.vue', frontend), 'utf8')
    assert.match(settings, /class="em-table email-example-table"[^>]*:dir="emailDirection\(emailTemplate\)"/s)
})

test('status history title is a translated phrase with both links as placeholders in every culture', async () => {
    const { readdirSync } = await import('node:fs')
    const resources = new URL('../WB.UI.Headquarters.Core/Resources/', frontend)
    const files = readdirSync(resources).filter(file => /^Pages(\.[a-z]{2})?\.resx$/.test(file))
    assert.ok(files.length >= 16, files.join())
    for (const file of files) {
        const match = readFileSync(new URL(file, resources), 'utf8')
            .match(/<data name="HistoryOfStatuses_InterviewResponsible"[^>]*>\s*<value>([^<]*)<\/value>/)
        assert.ok(match, file)
        assert.deepEqual(match[1].match(/\{\{\w+\}\}/g).sort(), ['{{interviewKey}}', '{{responsible}}'], file)
    }
    const view = readFileSync(new URL('src/hqapp/Views/HQ/Interviews/HqInterviews.vue', frontend), 'utf8')
    assert.doesNotMatch(view, /^\s+by\s*$/m)
    assert.match(view, /\$t\('Pages\.HistoryOfStatuses_InterviewResponsible'/)
})

import postcss from 'postcss'
import rtlcss from 'postcss-rtlcss'

export default function webInterviewRtl() {
    return {
        postcssPlugin: 'webinterview-rtl',
        async Once(root, { result }) {
            const file = result.opts.from?.replaceAll('\\', '/')
            if (!/\/assets\/css\/markup(?:-web-interview)?\.scss$/.test(file ?? '')) return

            await postcss([rtlcss({
                ltrPrefix: ':where(html:not(.web-interview-page[dir="rtl"]))',
                rtlPrefix: ':where(html.web-interview-page[dir="rtl"])',
                bothPrefix: ':where(html)',
                processDeclarationPlugins: [{
                    name: 'preserve-sprite-positions',
                    priority: 99,
                    processors: [{
                        // Background coordinates select physical pixels in checkbox and flag sprites.
                        expr: /^background-position(?:-x)?$/i,
                        action: (prop, value) => ({ prop, value }),
                    }],
                }],
            })]).process(root, { from: result.opts.from, map: false })
        },
    }
}

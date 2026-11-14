import postcss from 'postcss'
import rtlcss from 'postcss-rtlcss'

export default function webInterviewRtl() {
    return {
        postcssPlugin: 'webinterview-rtl',
        async Once(root, { result }) {
            const file = result.opts.from?.replaceAll('\\', '/')
            if (!/\/assets\/css\/markup(?:-web-interview|-interview-review)?\.scss$/.test(file ?? '')) return

            await postcss([rtlcss({
                ltrPrefix: ':where(html:not(.web-interview-page[dir="rtl"]))',
                rtlPrefix: ':where(html.web-interview-page[dir="rtl"])',
                bothPrefix: ':where(html)',
            })]).process(root, { from: result.opts.from, map: false })
        },
    }
}

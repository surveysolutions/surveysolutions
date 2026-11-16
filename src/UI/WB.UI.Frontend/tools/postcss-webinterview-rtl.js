import postcss from 'postcss'
import rtlcss from 'postcss-rtlcss'

export default function webInterviewRtl() {
    return {
        postcssPlugin: 'webinterview-rtl',
        async Once(root, { result }) {
            const file = result.opts.from?.replaceAll('\\', '/')
            if (!/\/assets\/css\/markup(?:-web-interview|-interview-review|-specific)?\.scss$/.test(file ?? '')) return

            await postcss([rtlcss({
                ltrPrefix: ':where(html:not([dir="rtl"]))',
                rtlPrefix: ':where(html[dir="rtl"])',
                bothPrefix: ':where(html)',
            })]).process(root, { from: result.opts.from, map: false })
        },
    }
}

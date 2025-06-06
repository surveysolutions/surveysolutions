export function registerLinkToRoute(vue, { router, store }) {
    vue.directive('linkToRoute', {
        beforeMount: (el, _, vnode) => {
            el.addEventListener('click', event => {
                // ensure we use the link, in case the click has been received by a subelement
                let { target } = event
                while (target && target.tagName !== 'A') target = target.parentNode
                // handle only links that do not reference external resources
                if (target && target.matches('a:not([href*=\'://\'])') && target.href) {
                    // some sanity checks taken from vue-router:
                    // https://github.com/vuejs/vue-router/blob/dev/src/components/link.js#L106
                    const { altKey, ctrlKey, metaKey, shiftKey, button, defaultPrevented } = event
                    // don't handle with control keys
                    if (metaKey || altKey || ctrlKey || shiftKey) return
                    // don't handle when preventDefault called
                    if (defaultPrevented) return
                    // don't handle right clicks
                    if (button !== undefined && button !== 0) return
                    // don't handle if `target="_blank"`
                    if (target && target.getAttribute) {
                        const linkTarget = target.getAttribute('target')
                        if (/\b_blank\b/i.test(linkTarget)) return
                    }
                    // don't handle same page links/anchors
                    const url = new URL(target.href)
                    let to = url.pathname
                    if (url.hash)
                        to = to + '#' + url.hash

                    if (url.protocol !== 'mailto:' && url.protocol !== 'tel:') {
                        if (window.location.pathname !== to && event.preventDefault) {
                            event.preventDefault()

                            // do not go into interview from take new page
                            if (store.getters.isTakeNewAssignment === true || to.includes('void(0)'))
                                return

                            if (to.startsWith('/api/')) {
                                window.open(target.href, '_blank')
                                return
                            }

                            let toPath = router.options.history.base == '/'
                                ? to
                                : to.replace(router.options.history.base, '')
                            if (!toPath.startsWith('/'))
                                toPath = '/' + toPath

                            router.push({ path: toPath })
                        }
                    }
                }
            })
        },
    })
}

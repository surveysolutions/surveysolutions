import assert from 'node:assert/strict'
import { readFile } from 'node:fs/promises'
import { test } from 'node:test'
import { SourceTextModule, SyntheticModule } from 'node:vm'
import { createMemoryHistory, createRouter } from 'vue-router'

const routerSource = await readFile(new URL('../../../src/webinterview/router/index.js', import.meta.url), 'utf8')

async function setup(coverPageId = 'cover-id') {
    const config = { coverPageId, splashScreen: false }
    const routeModule = new SourceTextModule(routerSource, {
        importModuleDynamically: async () => {
            const component = new SyntheticModule(['default'], function () {
                this.setExport('default', {})
            })
            await component.link(() => {})
            await component.evaluate()
            return component
        },
    })
    await routeModule.link(specifier => {
        const exports = specifier === 'vue-router'
            ? { createRouter, createWebHistory: createMemoryHistory }
            : { config }
        return new SyntheticModule(Object.keys(exports), function () {
            for (const [key, value] of Object.entries(exports))
                this.setExport(key, value)
        })
    })
    await routeModule.evaluate()

    let storedRoute
    const router = routeModule.namespace.default({
        commit(type, route) {
            assert.equal(type, 'SET_ROUTE')
            storedRoute = route
        },
        subscribeAction() {},
    })
    return { router, storedRoute: () => storedRoute }
}

// The router's afterEach hook checks the sidebar hamburger.
globalThis.document = { getElementById: () => null }

test('repeated Cover navigation preserves the section used for breadcrumbs and validation', async () => {
    const { router, storedRoute } = await setup()
    await router.push('/interview/Complete')
    await router.push({ name: 'cover' })
    assert.equal(storedRoute().params.sectionId, 'cover-id')

    await router.push({ name: 'cover' })
    assert.equal(router.currentRoute.value.params.sectionId, 'cover-id')
    assert.equal(storedRoute().params.sectionId, 'cover-id')
})

test('Cover hash and query changes preserve the section used for answer refreshes', async () => {
    const { router, storedRoute } = await setup()
    await router.push('/interview/Section/cover-id')
    for (const target of [
        '/interview/Section/cover-id#',
        '/interview/Section/cover-id#question',
        '/interview/Section/cover-id?resume=true',
    ]) {
        await router.push(target)
        assert.equal(storedRoute().params.sectionId, 'cover-id')
        assert.equal(router.currentRoute.value.params.sectionId, 'cover-id')
    }
})

test('normal sections, Complete, and legacy Cover keep their own section identity', async () => {
    const { router, storedRoute } = await setup()
    await router.push('/interview/Section/section-id')
    assert.equal(storedRoute().params.sectionId, 'section-id')
    await router.push('/interview/Complete')
    assert.equal(storedRoute().params.sectionId, undefined)

    const legacy = await setup(null)
    await legacy.router.push('/interview/Cover')
    assert.equal(legacy.storedRoute().params.sectionId, undefined)
})

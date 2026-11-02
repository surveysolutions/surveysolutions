export async function registerQuestionGlobalComponents(vue) {
    const { registerQuestionComponents } = await import('./components/questions')
    registerQuestionComponents(vue)
}

export function ensureQuestionGlobalComponents(vue) {
    if (vue.__wiQuestionComponentsRegistered) {
        return Promise.resolve()
    }

    if (!vue.__wiQuestionComponentsPromise) {
        vue.__wiQuestionComponentsPromise = registerQuestionGlobalComponents(vue)
            .then(() => {
                vue.__wiQuestionComponentsRegistered = true
            })
            .catch((error) => {
                delete vue.__wiQuestionComponentsPromise
                throw error
            })
    }

    return vue.__wiQuestionComponentsPromise
}

import { marked } from 'marked'

export function renderMarkdownWithQuestionnaireTitle(text, title, options = {}) {
    const renderer = options.renderer || new marked.Renderer()
    const renderText = renderer.text.bind(renderer)
    const questionnaireTitle = title ?? ''

    renderer.text = token => {
        if (token.tokens) return renderText(token)

        const text = token.text
            .replaceAll('%SURVEYNAME%', questionnaireTitle)
            .replaceAll('%QUESTIONNAIRE%', questionnaireTitle)
        return renderText({ ...token, text })
    }

    return marked(text, { ...options, renderer })
}

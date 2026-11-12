export function escapeMarkdownTitle(text, title) {
    if (!title) return text

    const escapedTitle = title.replace(/([!"#$%&'()*+,\-./:;<=>?@[\\\]^_`{|}~])/g, '\\$1')
    return text.split(title).join(escapedTitle)
}

export default {
    functional: true,
    render: function (createElement, context) {
        // Transparently pass any attributes, event listeners, children, etc.
        return createElement('button', context.data, context.children)
    }
}

<template>
    <div class="md-editor-container">
        <div v-if="editor" class="editor-toolbar">
            <button type="button" @click="setHeading(1)" :class="{ active: isActive.h1 }">
                H1
            </button>
            <button type="button" @click="setHeading(2)" :class="{ active: isActive.h2 }">
                H2
            </button>
            <button type="button" @click="toggleBold" :class="{ active: isActive.bold }">
                <strong>B</strong>
            </button>
            <button type="button" @click="toggleItalic" :class="{ active: isActive.italic }">
                <em>I</em>
            </button>
            <button type="button" @click="toggleBulletList" :class="{ active: isActive.bulletList }">
                • List
            </button>
            <button type="button" @click="toggleOrderedList" :class="{ active: isActive.orderedList }">
                1. List
            </button>
            <button type="button" @click="addImage">Image</button>
            <button type="button" @click="addLink" :class="{ active: isActive.link }">Link</button>
        </div>
        <div ref="editorElement" class="editor-content"></div>
    </div>
</template>


<style scoped>
.md-editor-container {
    border: 1px solid #ddd;
    border-radius: 4px;
}

.editor-toolbar {
    display: flex;
    gap: 4px;
    padding: 8px;
    border-bottom: 1px solid #ddd;
    background-color: #f5f5f5;
    flex-wrap: wrap;
}

.editor-toolbar button {
    padding: 6px 12px;
    border: 1px solid #ccc;
    background: white;
    border-radius: 3px;
    cursor: pointer;
    font-size: 14px;
}

.editor-toolbar button:hover {
    background-color: #e9e9e9;
}

.editor-toolbar button.active {
    background-color: #007bff;
    color: white;
    border-color: #0056b3;
}

.editor-content {
    min-height: 300px;
    padding: 12px;
}

:deep(.ProseMirror) {
    outline: none;
    min-height: 300px;
}

:deep(.ProseMirror p) {
    margin: 0 0 1em 0;
}

:deep(.ProseMirror h1),
:deep(.ProseMirror h2),
:deep(.ProseMirror h3),
:deep(.ProseMirror h4),
:deep(.ProseMirror h5),
:deep(.ProseMirror h6) {
    margin-top: 1.5em;
    margin-bottom: 0.5em;
    font-weight: bold;
}

:deep(.ProseMirror h1) {
    font-size: 2em;
}

:deep(.ProseMirror h2) {
    font-size: 1.5em;
}

:deep(.ProseMirror h3) {
    font-size: 1.17em;
}

:deep(.ProseMirror ul),
:deep(.ProseMirror ol) {
    padding-left: 2em;
    margin: 1em 0;
}

:deep(.ProseMirror img) {
    max-width: 100%;
    height: auto;
}

:deep(.ProseMirror a) {
    color: #007bff;
    text-decoration: underline;
}

:deep(.ProseMirror strong) {
    font-weight: bold;
}

:deep(.ProseMirror em) {
    font-style: italic;
}
</style>

<script>
import { Editor } from '@tiptap/core'
import StarterKit from '@tiptap/starter-kit'
import Image from '@tiptap/extension-image'
import Link from '@tiptap/extension-link'
import { Markdown } from 'tiptap-markdown'
import { escape, unescape } from 'lodash'
import { markRaw } from 'vue'

export default {
    emits: ['input', 'update:modelValue'],
    props: {
        modelValue: { type: String, required: true },
        supportHtml: { type: Boolean, required: false, default: false },
    },
    data() {
        return {
            editor: null,
            isUpdating: false,
            isActive: {
                h1: false,
                h2: false,
                bold: false,
                italic: false,
                bulletList: false,
                orderedList: false,
                link: false
            }
        }
    },
    mounted() {
        this.initializeEditor()
    },
    unmounted() {
        if (this.editor) {
            this.editor.destroy()
            this.editor = null
        }
    },
    watch: {
        modelValue(newValue) {
            if (this.isUpdating || !this.editor) return

            let processedValue = this.supportHtml ? newValue : unescape(newValue)
            const currentContent = this.getEditorContent()

            if (currentContent !== processedValue) {
                this.setEditorContent(processedValue)
            }
        }
    },
    expose: ['refresh'],
    methods: {
        initializeEditor() {
            let initialContent = this.modelValue
            if (!this.supportHtml) {
                initialContent = unescape(initialContent)
            }

            this.editor = markRaw(new Editor({
                element: this.$refs.editorElement,
                extensions: [
                    StarterKit.configure({
                        heading: {
                            levels: [1, 2, 3, 4, 5, 6]
                        }
                    }),
                    Image,
                    Link.configure({
                        openOnClick: false,
                        HTMLAttributes: {
                            target: '_blank',
                            rel: 'noopener noreferrer'
                        }
                    }),
                    Markdown.configure({
                        html: true,
                        transformPastedText: true,
                        transformCopiedText: false,
                    })
                ],
                content: initialContent,
                onUpdate: () => {
                    this.handleContentChange()
                },
                onSelectionUpdate: ({ editor }) => {
                    this.updateActiveStates(editor)
                },
                onFocus: ({ editor }) => {
                    this.updateActiveStates(editor)
                }
            }))

            // Initial state
            this.updateActiveStates(this.editor)
        },
        updateActiveStates(editor) {
            if (!editor) return

            this.isActive = {
                h1: editor.isActive('heading', { level: 1 }),
                h2: editor.isActive('heading', { level: 2 }),
                bold: editor.isActive('bold'),
                italic: editor.isActive('italic'),
                bulletList: editor.isActive('bulletList'),
                orderedList: editor.isActive('orderedList'),
                link: editor.isActive('link')
            }
        },
        getEditorContent() {
            if (!this.editor) return ''
            return this.editor.storage.markdown.getMarkdown()
        },
        setEditorContent(content) {
            if (!this.editor) return
            this.editor.commands.setContent(content, false)
        },
        handleContentChange() {
            if (!this.editor) return

            this.isUpdating = true
            let content = this.getEditorContent()

            if (!this.supportHtml) {
                content = escape(content)
            }

            this.$emit('update:modelValue', content)
            this.$emit('input', content)

            this.$nextTick(() => {
                this.isUpdating = false
            })
        },
        setHeading(level) {
            this.editor.chain().focus().toggleHeading({ level }).run()
        },
        toggleBold() {
            this.editor.chain().focus().toggleBold().run()
        },
        toggleItalic() {
            this.editor.chain().focus().toggleItalic().run()
        },
        toggleBulletList() {
            this.editor.chain().focus().toggleBulletList().run()
        },
        toggleOrderedList() {
            this.editor.chain().focus().toggleOrderedList().run()
        },
        addImage() {
            const input = document.createElement('input')
            input.type = 'file'
            input.accept = 'image/*'
            input.onchange = (e) => {
                const file = e.target.files[0]
                if (file) {
                    const reader = new FileReader()
                    reader.onload = (event) => {
                        const base64 = event.target.result
                        this.editor.chain().focus().setImage({ src: base64 }).run()
                    }
                    reader.readAsDataURL(file)
                }
            }
            input.click()
        },
        addLink() {
            const url = window.prompt('Enter link URL:')
            if (url) {
                this.editor.chain().focus().setLink({ href: url }).run()
            } else if (this.editor.isActive('link')) {
                this.editor.chain().focus().unsetLink().run()
            }
        },
        refresh() {
            setTimeout(() => {
                if (this.editor) {
                    this.editor.commands.focus('start')
                }
            }, 100)
        }
    }
}
</script>
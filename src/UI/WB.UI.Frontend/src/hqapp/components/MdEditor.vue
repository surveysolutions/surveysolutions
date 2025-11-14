<template>
    <div class="md-editor-container">
        <div v-if="editor" class="editor-toolbar">
            <div class="dropdown">
                <button type="button" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false"
                    :class="{ active: isActive.heading }" :aria-pressed="isActive.heading" aria-label="Heading"
                    title="Heading">
                    <span class="glyphicon glyphicon-header"></span>
                </button>
                <ul class="dropdown-menu">
                    <li><a href="javascript:void(0);" @click="setHeading(1)">
                            <h1>Heading 1</h1>
                        </a></li>
                    <li><a href="javascript:void(0);" @click="setHeading(2)">
                            <h2>Heading 2</h2>
                        </a></li>
                    <li><a href="javascript:void(0);" @click="setHeading(3)">
                            <h3>Heading 3</h3>
                        </a></li>
                    <li><a href="javascript:void(0);" @click="setHeading(4)">
                            <h4>Heading 4</h4>
                        </a></li>
                    <li><a href="javascript:void(0);" @click="setHeading(5)">
                            <h5>Heading 5</h5>
                        </a></li>
                    <li><a href="javascript:void(0);" @click="setHeading(6)">
                            <h6>Heading 6</h6>
                        </a></li>
                </ul>
            </div>
            <button type="button" @click="toggleBold" :class="{ active: isActive.bold }" :aria-pressed="isActive.bold"
                aria-label="Bold" title="Bold">
                <span class="glyphicon glyphicon-bold"></span>
            </button>
            <button type="button" @click="toggleItalic" :class="{ active: isActive.italic }"
                :aria-pressed="isActive.italic" aria-label="Italic" title="Italic">
                <span class="glyphicon glyphicon-italic"></span>
            </button>
            <button type="button" @click="toggleBulletList" :class="{ active: isActive.bulletList }"
                :aria-pressed="isActive.bulletList" aria-label="Bullet List" title="Bullet List">
                <span class="glyphicon glyphicon-list"></span>
            </button>
            <button type="button" @click="toggleOrderedList" :class="{ active: isActive.orderedList }"
                :aria-pressed="isActive.orderedList" aria-label="Ordered List" title="Ordered List">
                <span class="glyphicon glyphicon-sort-by-order"></span>
            </button>
            <button type="button" @click="addImage" aria-label="Insert Image" title="Insert Image">
                <span class="glyphicon glyphicon-picture"></span>
            </button>
            <button type="button" @click="openLinkModal" :class="{ active: isActive.link }"
                :aria-pressed="isActive.link" aria-label="Insert Link" title="Insert Link">
                <span class="glyphicon glyphicon-link"></span>
            </button>
        </div>
        <div ref="editorElement" class="editor-content"></div>

        <!-- Link Modal -->
        <div class="modal fade" :class="{ in: showLinkModal }" tabindex="-1" role="dialog"
            :style="{ display: showLinkModal ? 'block' : 'none' }">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" @click="closeLinkModal" aria-label="Close">
                            <span aria-hidden="true"></span>
                        </button>
                        <h4 class="modal-title">{{ isActive.link ? 'Edit Link' : 'Add Link' }}</h4>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <label for="linkText">Link Text</label>
                            <input type="text" id="linkText" v-model="linkText" class="form-control"
                                placeholder="Enter link text" @keyup.enter="isLinkFormValid && applyLink()" />
                        </div>
                        <div class="form-group">
                            <label for="linkUrl">URL</label>
                            <input type="text" id="linkUrl" v-model="linkUrl" class="form-control"
                                placeholder="https://example.com" @keyup.enter="isLinkFormValid && applyLink()" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-success" @click="applyLink"
                            :disabled="!isLinkFormValid">OK</button>
                        <button type="button" class="btn btn-link" @click="closeLinkModal">Cancel</button>
                    </div>
                </div>
            </div>
        </div>
        <div class="modal-backdrop in" :style="{ display: showLinkModal ? 'block' : 'none' }"></div>

        <!-- Image Modal -->
        <div class="modal fade" :class="{ in: showImageModal }" tabindex="-1" role="dialog"
            :style="{ display: showImageModal ? 'block' : 'none' }">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" @click="closeImageModal" aria-label="Close">
                            <span aria-hidden="true"></span>
                        </button>
                        <h4 class="modal-title">Insert Image</h4>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <label for="imageFile">Image File</label>
                            <input type="file" id="imageFile" @change="handleImageFileChange" class="form-control"
                                accept="image/*" />
                            <small class="help-block" v-if="imagePreview">Preview:</small>
                            <img v-if="imagePreview" :src="imagePreview" alt="Preview"
                                style="max-width: 100%; max-height: 200px; margin-top: 10px;" />
                        </div>
                        <div class="form-group">
                            <label for="imageDescription">Description (Alt Text)</label>
                            <input type="text" id="imageDescription" v-model="imageDescription" class="form-control"
                                placeholder="Enter image description" @keyup.enter="isImageFormValid && applyImage()" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-success" @click="applyImage"
                            :disabled="!isImageFormValid">OK</button>
                        <button type="button" class="btn btn-link" @click="closeImageModal">Cancel</button>
                    </div>
                </div>
            </div>
        </div>
        <div class="modal-backdrop in" :style="{ display: showImageModal ? 'block' : 'none' }"></div>
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

.editor-toolbar .dropdown {
    position: relative;
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

.editor-toolbar .dropdown-menu {
    display: none;
    position: absolute;
    top: 100%;
    left: 0;
    z-index: 1000;
    min-width: 160px;
    padding: 5px 0;
    margin: 2px 0 0;
    background-color: #fff;
    border: 1px solid #ccc;
    border-radius: 4px;
    box-shadow: 0 6px 12px rgba(0, 0, 0, 0.175);
    list-style: none;
}

.editor-toolbar .dropdown.show .dropdown-menu,
.editor-toolbar .dropdown-menu.show {
    display: block;
}

.editor-toolbar .dropdown-menu li {
    margin: 0;
    padding: 0;
}

.editor-toolbar .dropdown-menu li a {
    display: block;
    padding: 8px 20px;
    clear: both;
    font-weight: normal;
    line-height: 1.42857143;
    color: #333;
    white-space: nowrap;
    text-decoration: none;
    cursor: pointer;
}

.editor-toolbar .dropdown-menu li a:hover,
.editor-toolbar .dropdown-menu li a:focus {
    background-color: #f5f5f5;
    color: #262626;
}

.editor-toolbar .dropdown-menu li a h1,
.editor-toolbar .dropdown-menu li a h2,
.editor-toolbar .dropdown-menu li a h3,
.editor-toolbar .dropdown-menu li a h4,
.editor-toolbar .dropdown-menu li a h5,
.editor-toolbar .dropdown-menu li a h6 {
    margin: 0;
    padding: 0;
    font-weight: bold;
    line-height: 1.42857143;
}

.editor-toolbar .dropdown-menu li a h1 {
    font-size: 2em;
}

.editor-toolbar .dropdown-menu li a h2 {
    font-size: 1.5em;
}

.editor-toolbar .dropdown-menu li a h3 {
    font-size: 1.17em;
}

.editor-toolbar .dropdown-menu li a h4 {
    font-size: 1em;
}

.editor-toolbar .dropdown-menu li a h5 {
    font-size: 0.83em;
}

.editor-toolbar .dropdown-menu li a h6 {
    font-size: 0.67em;
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
                heading: false,
                bold: false,
                italic: false,
                bulletList: false,
                orderedList: false,
                link: false
            },
            showLinkModal: false,
            linkUrl: '',
            linkText: '',
            showImageModal: false,
            imageFile: null,
            imagePreview: '',
            imageDescription: ''
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
    computed: {
        isLinkFormValid() {
            return this.linkText.trim() !== '' && this.linkUrl.trim() !== ''
        },
        isImageFormValid() {
            return this.imageFile !== null || this.imagePreview !== ''
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
                heading: editor.isActive('heading'),
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
            this.openImageModal()
        },
        openImageModal() {
            this.showImageModal = true
            this.imageFile = null
            this.imagePreview = ''
            this.imageDescription = ''
            this.$nextTick(() => {
                const input = document.getElementById('imageFile')
                if (input) input.focus()
            })
        },
        closeImageModal() {
            this.showImageModal = false
            this.imageFile = null
            this.imagePreview = ''
            this.imageDescription = ''
        },
        handleImageFileChange(event) {
            const file = event.target.files[0]
            if (file) {
                this.imageFile = file
                const reader = new FileReader()
                reader.onload = (e) => {
                    this.imagePreview = e.target.result
                }
                reader.readAsDataURL(file)
            } else {
                this.imageFile = null
                this.imagePreview = ''
            }
        },
        applyImage() {
            if (!this.imagePreview) {
                this.closeImageModal()
                return
            }

            const imageAttrs = {
                src: this.imagePreview
            }

            if (this.imageDescription.trim()) {
                imageAttrs.alt = this.imageDescription.trim()
            }

            this.editor.chain().focus().setImage(imageAttrs).run()
            this.closeImageModal()
        },
        openLinkModal() {
            const { from, to } = this.editor.state.selection
            const selectedText = this.editor.state.doc.textBetween(from, to, '')

            if (this.editor.isActive('link')) {
                // Editing existing link
                this.linkUrl = this.editor.getAttributes('link').href || ''
                this.linkText = selectedText
            } else {
                // Creating new link
                this.linkUrl = ''
                this.linkText = selectedText
            }

            this.showLinkModal = true
            this.$nextTick(() => {
                const input = this.linkText ? document.getElementById('linkUrl') : document.getElementById('linkText')
                if (input) input.focus()
            })
        },
        closeLinkModal() {
            this.showLinkModal = false
            this.linkUrl = ''
            this.linkText = ''
        },
        applyLink() {
            if (!this.linkUrl) {
                this.closeLinkModal()
                return
            }

            const { from, to } = this.editor.state.selection

            if (this.linkText) {
                // Replace selection with link text and apply link
                this.editor
                    .chain()
                    .focus()
                    .deleteSelection()
                    .insertContent(this.linkText)
                    .setTextSelection({ from, to: from + this.linkText.length })
                    .setLink({ href: this.linkUrl })
                    .run()
            } else {
                // Just apply link to existing selection
                this.editor.chain().focus().setLink({ href: this.linkUrl }).run()
            }

            this.closeLinkModal()
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
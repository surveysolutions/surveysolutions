<template>
    <div class="md-editor-container">
        <div class="editor-toolbar">
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
        <textarea ref="textarea" @input="onInput" class="markdown-editor"></textarea>

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
                            <input type="text" id="linkText" ref="linkText" v-model="linkText" class="form-control"
                                placeholder="Enter link text" @keyup.enter="isLinkFormValid && applyLink()" />
                        </div>
                        <div class="form-group">
                            <label for="linkUrl">URL</label>
                            <input type="text" id="linkUrl" ref="linkUrl" v-model="linkUrl" class="form-control"
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
                            <input type="file" id="imageFile" ref="imageFile" @change="handleImageFileChange"
                                class="form-control" accept="image/*" />
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

.markdown-editor {
    width: 100%;
    min-height: 300px;
    border: none;
    padding: 12px;
    font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', 'Consolas', monospace;
    font-size: 14px;
    line-height: 1.5;
    resize: vertical;
    outline: none;
}

.markdown-editor:focus {
    outline: none;
}
</style>

<script>
import { escape, unescape } from 'lodash'

export default {
    emits: ['input', 'update:modelValue'],
    props: {
        modelValue: { type: String, required: true },
        supportHtml: { type: Boolean, required: false, default: false },
    },
    data() {
        return {
            content: '',
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
        let val = this.modelValue
        if (this.supportHtml != true) {
            val = unescape(val)
        }
        this.content = val
        this.$refs.textarea.value = val
    },
    watch: {
        modelValue(newValue) {
            let val = newValue
            if (this.supportHtml != true) {
                val = unescape(val)
            }
            if (val !== this.content) {
                const textarea = this.$refs.textarea
                if (textarea) {
                    const cursorPos = textarea.selectionStart
                    this.content = val
                    textarea.value = val
                    this.$nextTick(() => {
                        textarea.setSelectionRange(cursorPos, cursorPos)
                    })
                } else {
                    this.content = val
                }
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
        onInput() {
            const textarea = this.$refs.textarea
            this.content = textarea.value
            let markDown = this.content

            if (this.supportHtml != true) {
                markDown = escape(markDown)
            }

            if (this.modelValue != markDown) {
                this.$emit('input', markDown)
                this.$emit('update:modelValue', markDown)
            }
        },
        getSelection() {
            const textarea = this.$refs.textarea
            return {
                start: textarea.selectionStart,
                end: textarea.selectionEnd,
                text: textarea.value.substring(textarea.selectionStart, textarea.selectionEnd)
            }
        },
        insertAtCursor(text, cursorOffset = text.length) {
            const textarea = this.$refs.textarea
            const start = textarea.selectionStart
            const end = textarea.selectionEnd

            textarea.focus()

            // Use document.execCommand to maintain undo history
            if (document.execCommand) {
                document.execCommand('insertText', false, text)
            } else {
                // Fallback for browsers that don't support execCommand
                const before = textarea.value.substring(0, start)
                const after = textarea.value.substring(end)
                textarea.value = before + text + after
                this.content = textarea.value
            }

            const newCursorPos = start + cursorOffset
            textarea.setSelectionRange(newCursorPos, newCursorPos)

            // Trigger input event manually if needed
            if (!document.execCommand) {
                this.onInput()
            }
        },
        wrapSelection(prefix, suffix) {
            const textarea = this.$refs.textarea
            const selection = this.getSelection()

            textarea.focus()

            if (selection.text) {
                // Replace selected text with wrapped version
                const wrappedText = prefix + selection.text + suffix

                if (document.execCommand) {
                    document.execCommand('insertText', false, wrappedText)
                } else {
                    const before = textarea.value.substring(0, selection.start)
                    const after = textarea.value.substring(selection.end)
                    textarea.value = before + wrappedText + after
                    this.content = textarea.value
                    this.onInput()
                }

                const newStart = selection.start + prefix.length
                const newEnd = selection.start + prefix.length + selection.text.length
                textarea.setSelectionRange(newStart, newEnd)
            } else {
                const placeholder = 'text'
                const wrappedText = prefix + placeholder + suffix

                if (document.execCommand) {
                    document.execCommand('insertText', false, wrappedText)
                } else {
                    const before = textarea.value.substring(0, selection.start)
                    const after = textarea.value.substring(selection.end)
                    textarea.value = before + wrappedText + after
                    this.content = textarea.value
                    this.onInput()
                }

                const newPos = selection.start + prefix.length + placeholder.length
                textarea.setSelectionRange(newPos, newPos)
            }
        },
        insertMarkdown(prefix, suffix) {
            const textarea = this.$refs.textarea
            const start = textarea.selectionStart
            const lineStart = textarea.value.lastIndexOf('\n', start - 1) + 1

            textarea.focus()
            textarea.setSelectionRange(lineStart, lineStart)

            if (document.execCommand) {
                document.execCommand('insertText', false, prefix)
            } else {
                const before = textarea.value.substring(0, lineStart)
                const after = textarea.value.substring(lineStart)
                textarea.value = before + prefix + after
                this.content = textarea.value
                this.onInput()
            }

            const newCursorPos = lineStart + prefix.length
            textarea.setSelectionRange(newCursorPos, newCursorPos)
        },
        insertList(prefix, isOrdered = false) {
            const selection = this.getSelection()
            const textarea = this.$refs.textarea

            if (selection.text.includes('\n')) {
                // Multi-line selection
                const lines = selection.text.split('\n')
                let formatted
                if (isOrdered) {
                    formatted = lines.map((line, index) => line.trim() ? `${index + 1}. ${line}` : line).join('\n')
                } else {
                    formatted = lines.map(line => line.trim() ? prefix + line : line).join('\n')
                }

                textarea.focus()

                if (document.execCommand) {
                    document.execCommand('insertText', false, formatted)
                } else {
                    const before = textarea.value.substring(0, selection.start)
                    const after = textarea.value.substring(selection.end)
                    textarea.value = before + formatted + after
                    this.content = textarea.value
                    this.onInput()
                }

                const newEnd = selection.start + formatted.length
                textarea.setSelectionRange(selection.start, newEnd)
            } else {
                // Single line - find the next number if ordered list
                let actualPrefix = prefix
                if (isOrdered) {
                    const start = textarea.selectionStart
                    const lineStart = textarea.value.lastIndexOf('\n', start - 1) + 1
                    const beforeLine = textarea.value.substring(0, lineStart)
                    const lines = beforeLine.split('\n')

                    // Look backwards to find the last ordered list item
                    let lastNumber = 0
                    for (let i = lines.length - 1; i >= 0; i--) {
                        const match = lines[i].match(/^(\d+)\.\s/)
                        if (match) {
                            lastNumber = parseInt(match[1])
                            break
                        } else if (lines[i].trim() !== '') {
                            // Hit a non-empty, non-list line - stop looking
                            break
                        }
                    }
                    actualPrefix = `${lastNumber + 1}. `
                }

                const start = textarea.selectionStart
                const lineStart = textarea.value.lastIndexOf('\n', start - 1) + 1

                textarea.focus()
                textarea.setSelectionRange(lineStart, lineStart)

                if (document.execCommand) {
                    document.execCommand('insertText', false, actualPrefix)
                } else {
                    const before = textarea.value.substring(0, lineStart)
                    const after = textarea.value.substring(lineStart)
                    textarea.value = before + actualPrefix + after
                    this.content = textarea.value
                    this.onInput()
                }

                const newCursorPos = lineStart + actualPrefix.length
                textarea.setSelectionRange(newCursorPos, newCursorPos)
            }
        },
        setHeading(level) {
            const prefix = '#'.repeat(level) + ' '
            this.insertMarkdown(prefix, '')
        },
        toggleBold() {
            this.wrapSelection('**', '**')
        },
        toggleItalic() {
            this.wrapSelection('*', '*')
        },
        toggleBulletList() {
            this.insertList('- ', false)
        },
        toggleOrderedList() {
            this.insertList('1. ', true)
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
                const input = this.$refs.imageFile
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

            const altText = this.imageDescription.trim() || 'image'
            const markdown = `![${altText}](${this.imagePreview})`
            this.insertAtCursor(markdown)
            this.closeImageModal()
        },
        openLinkModal() {
            const selection = this.getSelection()
            this.linkText = selection.text
            this.linkUrl = ''

            this.showLinkModal = true
            this.$nextTick(() => {
                const input = this.linkText ? this.$refs.linkUrl : this.$refs.linkText
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

            const selection = this.getSelection()
            const linkText = this.linkText || 'link text'
            const markdown = `[${linkText}](${this.linkUrl})`

            const textarea = this.$refs.textarea
            textarea.focus()

            if (document.execCommand) {
                document.execCommand('insertText', false, markdown)
            } else {
                const before = textarea.value.substring(0, selection.start)
                const after = textarea.value.substring(selection.end)
                textarea.value = before + markdown + after
                this.content = textarea.value
                this.onInput()
            }

            const newCursorPos = selection.start + markdown.length
            textarea.setSelectionRange(newCursorPos, newCursorPos)

            this.closeLinkModal()
        },
        refresh() {
            setTimeout(() => {
                this.$refs.textarea.focus()
                this.$refs.textarea.setSelectionRange(0, 0)
            }, 100)
        }
    }
}
</script>
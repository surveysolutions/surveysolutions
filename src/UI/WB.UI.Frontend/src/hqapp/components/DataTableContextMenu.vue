<template>
    <Teleport to="body">
        <ul v-if="visible" ref="menuEl" class="context-menu-list" :style="menuStyle" role="menu">
            <template v-for="(item, i) in items" :key="i">
                <li v-if="isSeparator(item)" class="context-menu-separator context-menu-not-selectable"
                    role="separator"></li>
                <li v-else class="context-menu-item"
                    :class="[item.className, { 'context-menu-disabled': item.disabled, 'context-menu-hover': hoveredIndex === i }]"
                    role="menuitem" @mouseenter="hoveredIndex = i" @mouseleave="hoveredIndex = -1"
                    @click="onItemClick(item)">
                    {{ item.name }}
                </li>
            </template>
        </ul>
    </Teleport>
</template>

<script>
export default {
    name: 'DataTableContextMenu',

    props: {
        visible: {
            type: Boolean,
            default: false,
        },
        items: {
            type: Array,
            default: () => [],
        },
        x: {
            type: Number,
            default: 0,
        },
        y: {
            type: Number,
            default: 0,
        },
    },

    emits: ['close'],

    data() {
        return {
            hoveredIndex: -1,
            adjustedX: 0,
            adjustedY: 0,
        }
    },

    computed: {
        menuStyle() {
            return {
                position: 'fixed',
                left: this.adjustedX + 'px',
                top: this.adjustedY + 'px',
            }
        },
    },

    watch: {
        visible(newVal) {
            if (newVal) {
                this.hoveredIndex = -1
                this.adjustedX = this.x
                this.adjustedY = this.y
                this.$nextTick(() => this.clampToViewport())
                document.addEventListener('click', this.onOutsideClick, true)
                document.addEventListener('keydown', this.onKeyDown)
            } else {
                document.removeEventListener('click', this.onOutsideClick, true)
                document.removeEventListener('keydown', this.onKeyDown)
            }
        },
    },

    beforeUnmount() {
        document.removeEventListener('click', this.onOutsideClick, true)
        document.removeEventListener('keydown', this.onKeyDown)
    },

    methods: {
        isSeparator(item) {
            return item === '---------' ||
                (typeof item === 'object' && !item.name &&
                    item.className && item.className.includes('context-menu-separator'))
        },

        onItemClick(item) {
            if (item.disabled) return
            this.$emit('close')
            if (typeof item.callback === 'function') item.callback()
        },

        onOutsideClick(event) {
            if (this.$refs.menuEl && !this.$refs.menuEl.contains(event.target)) {
                this.$emit('close')
            }
        },

        onKeyDown(event) {
            if (event.key === 'Escape') {
                this.$emit('close')
            }
        },

        clampToViewport() {
            const el = this.$refs.menuEl
            if (!el) return
            const rect = el.getBoundingClientRect()
            const vw = window.innerWidth
            const vh = window.innerHeight
            if (rect.right > vw) this.adjustedX = Math.max(0, this.x - rect.width)
            if (rect.bottom > vh) this.adjustedY = Math.max(0, this.y - rect.height)
        },
    },
}
</script>

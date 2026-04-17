<template>
    <div :class="wrapperClasses" ref="triggerElement" @mouseenter="onTriggerMouseEnter"
        @mouseleave="onTriggerMouseLeave" @focusin="onTriggerFocus" @focusout="onTriggerBlur">
        <slot></slot>
        <Teleport v-if="enable && isVisible" :to="teleportTarget">
            <div class="popover bs5-popover show" :style="popoverStyle" @mouseenter="onPopoverMouseEnter"
                @mouseleave="onPopoverMouseLeave" ref="popoverElement">
                <div :class="`popover-arrow arrow-${placement}`" :style="arrowStyle"></div>
                <div class="popover-body">
                    <slot name="popover"></slot>
                </div>
            </div>
        </Teleport>
    </div>
</template>

<script>
export default {
    name: 'Bootstrap5Popover',
    inheritAttrs: false,
    props: {
        trigger: {
            type: String,
            default: 'hover'
        },
        enable: {
            type: Boolean,
            default: true
        },
        appendTo: {
            type: String,
            default: 'body'
        },
        placement: {
            type: String,
            default: 'top'
        }
    },
    data() {
        return {
            isVisible: false,
            isHovering: false,
            isFocused: false,
            popoverStyle: {},
            arrowStyle: {},
            hasPositionListeners: false
        }
    },
    computed: {
        wrapperClasses() {
            return this.$attrs.class || ''
        },
        teleportTarget() {
            return this.appendTo === 'body' ? 'body' : this.appendTo
        }
    },
    watch: {
        enable(newVal) {
            if (!newVal) {
                this.hidePopover()
            }
        }
    },
    beforeUnmount() {
        this.removePositionListeners()
    },
    methods: {
        onTriggerMouseEnter() {
            this.isHovering = true
            this.showPopover()
        },
        onTriggerMouseLeave() {
            this.isHovering = false
            if (!this.isFocused) {
                this.hidePopover()
            }
        },
        onTriggerFocus() {
            this.isFocused = true
            this.showPopover()
        },
        onTriggerBlur() {
            this.isFocused = false
            if (!this.isHovering) {
                this.hidePopover()
            }
        },
        onPopoverMouseEnter() {
            this.isHovering = true
        },
        onPopoverMouseLeave() {
            this.isHovering = false
            if (!this.isFocused) {
                this.hidePopover()
            }
        },
        showPopover() {
            if (!this.enable) return
            this.isVisible = true
            this.$nextTick(() => {
                this.addPositionListeners()
                this.updatePosition()
            })
        },
        hidePopover() {
            this.isVisible = false
            this.removePositionListeners()
        },
        addPositionListeners() {
            if (this.hasPositionListeners) return

            window.addEventListener('scroll', this.updatePosition, true)
            window.addEventListener('resize', this.updatePosition)
            this.hasPositionListeners = true
        },
        removePositionListeners() {
            if (!this.hasPositionListeners) return

            window.removeEventListener('scroll', this.updatePosition, true)
            window.removeEventListener('resize', this.updatePosition)
            this.hasPositionListeners = false
        },
        updatePosition() {
            if (!this.$refs.triggerElement || !this.$refs.popoverElement) return

            const trigger = this.$refs.triggerElement
            const popover = this.$refs.popoverElement

            const triggerRect = trigger.getBoundingClientRect()
            const popoverRect = popover.getBoundingClientRect()

            let top = 0
            let left = 0
            const offset = 8

            switch (this.placement) {
                case 'top':
                    top = triggerRect.top - popoverRect.height - offset
                    left = triggerRect.left + (triggerRect.width - popoverRect.width) / 2
                    break
                case 'bottom':
                    top = triggerRect.bottom + offset
                    left = triggerRect.left + (triggerRect.width - popoverRect.width) / 2
                    break
                case 'left':
                    top = triggerRect.top + (triggerRect.height - popoverRect.height) / 2
                    left = triggerRect.left - popoverRect.width - offset
                    break
                case 'right':
                    top = triggerRect.top + (triggerRect.height - popoverRect.height) / 2
                    left = triggerRect.right + offset
                    break
            }

            this.popoverStyle = {
                position: 'fixed',
                top: top + 'px',
                left: left + 'px',
                zIndex: 1070
            }
        }
    }
}
</script>

<style scoped>
.popover-body {
    padding: 0.5rem 0.75rem;
}

.bs5-popover {
    position: fixed;
    top: 0;
    left: 0;
    z-index: 1070;
    display: block;
    max-width: 276px;
    padding: 0;
    margin: 0;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, "Noto Sans", "Liberation Sans", sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol", "Noto Color Emoji";
    font-size: 0.875rem;
    font-weight: 400;
    line-height: 1.5;
    text-align: left;
    text-decoration: none;
    text-shadow: none;
    text-transform: none;
    letter-spacing: normal;
    word-break: normal;
    word-spacing: normal;
    white-space: normal;
    line-break: auto;
    font-size-adjust: none;
    font-variant-ligatures: normal;
    font-variant-caps: normal;
    font-variant-alts: normal;
    font-variant-numeric: normal;
    font-variant-east-asian: normal;
    font-variant-position: normal;
    font-feature-settings: normal;
    font-optical-sizing: auto;
    font-variation-settings: normal;
    background-color: #fff;
    background-clip: padding-box;
    border: 1px solid rgba(0, 0, 0, 0.176);
    border-radius: 0.25rem;
    box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
}

.popover-arrow {
    position: absolute;
    display: block;
    width: 0.8rem;
    height: 0.4rem;
}

.popover-arrow::before,
.popover-arrow::after {
    position: absolute;
    display: block;
    content: "";
    border-color: transparent;
    border-style: solid;
}

/* Top arrow */
.bs5-popover.top>.popover-arrow,
.bs5-popover>.arrow-top {
    bottom: calc(-0.4rem - 1px);
    left: 50%;
    transform: translateX(-50%);
}

.bs5-popover.top>.popover-arrow::before,
.bs5-popover>.arrow-top::before {
    bottom: -1px;
    left: 50%;
    transform: translateX(-50%);
    border-width: 0.4rem 0.4rem 0;
    border-top-color: rgba(0, 0, 0, 0.176);
}

.bs5-popover.top>.popover-arrow::after,
.bs5-popover>.arrow-top::after {
    bottom: 0;
    left: 50%;
    transform: translateX(-50%);
    border-width: 0.4rem 0.4rem 0;
    border-top-color: #fff;
}

/* Bottom arrow */
.bs5-popover.bottom>.popover-arrow,
.bs5-popover>.arrow-bottom {
    top: calc(-0.4rem - 1px);
    left: 50%;
    transform: translateX(-50%);
}

.bs5-popover.bottom>.popover-arrow::before,
.bs5-popover>.arrow-bottom::before {
    top: -1px;
    left: 50%;
    transform: translateX(-50%);
    border-width: 0 0.4rem 0.4rem;
    border-bottom-color: rgba(0, 0, 0, 0.176);
}

.bs5-popover.bottom>.popover-arrow::after,
.bs5-popover>.arrow-bottom::after {
    top: 0;
    left: 50%;
    transform: translateX(-50%);
    border-width: 0 0.4rem 0.4rem;
    border-bottom-color: #fff;
}

/* Left arrow */
.bs5-popover.left>.popover-arrow,
.bs5-popover>.arrow-left {
    right: calc(-0.4rem - 1px);
    top: 50%;
    transform: translateY(-50%);
}

.bs5-popover.left>.popover-arrow::before,
.bs5-popover>.arrow-left::before {
    right: -1px;
    top: 50%;
    transform: translateY(-50%);
    border-width: 0.4rem 0 0.4rem 0.4rem;
    border-left-color: rgba(0, 0, 0, 0.176);
}

.bs5-popover.left>.popover-arrow::after,
.bs5-popover>.arrow-left::after {
    right: 0;
    top: 50%;
    transform: translateY(-50%);
    border-width: 0.4rem 0 0.4rem 0.4rem;
    border-left-color: #fff;
}

/* Right arrow */
.bs5-popover.right>.popover-arrow,
.bs5-popover>.arrow-right {
    left: calc(-0.4rem - 1px);
    top: 50%;
    transform: translateY(-50%);
}

.bs5-popover.right>.popover-arrow::before,
.bs5-popover>.arrow-right::before {
    left: -1px;
    top: 50%;
    transform: translateY(-50%);
    border-width: 0.4rem 0.4rem 0.4rem 0;
    border-right-color: rgba(0, 0, 0, 0.176);
}

.bs5-popover.right>.popover-arrow::after,
.bs5-popover>.arrow-right::after {
    left: 0;
    top: 50%;
    transform: translateY(-50%);
    border-width: 0.4rem 0.4rem 0.4rem 0;
    border-right-color: #fff;
}
</style>

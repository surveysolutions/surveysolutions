<template>
    <div class="query-builder-group">
        <slot name="groupOperator" v-bind="groupController" />

        <div :class="groupChildrenClass">
            <div v-for="(child, index) in safeChildren" :key="childKey(child)" class="query-builder-child">
                <local-query-builder-group-node v-if="isGroup(child)" :group="child" :config="config"
                    :depth="depth + 1">
                    <template #groupOperator="slotProps">
                        <slot name="groupOperator" v-bind="slotProps" />
                    </template>

                    <template #groupControl="slotProps">
                        <slot name="groupControl" v-bind="slotProps" />
                    </template>

                    <template #rule="slotProps">
                        <slot name="rule" v-bind="slotProps" />
                    </template>
                </local-query-builder-group-node>

                <slot v-else name="rule" v-bind="ruleController(child)" />

                <button type="button" class="query-builder-child__delete-child" @click="removeChild(index)"
                    v-text="deleteLabel(child)"></button>
            </div>
        </div>

        <slot name="groupControl" v-bind="groupController" />
    </div>
</template>

<script>
let _nextId = 1
const nextId = () => _nextId++

const emptyGroup = operatorIdentifier => ({
    operatorIdentifier,
    children: [],
    _id: nextId(),
})

export default {
    name: 'LocalQueryBuilderGroupNode',

    props: {
        group: {
            type: Object,
            required: true,
        },
        config: {
            type: Object,
            required: true,
        },
        depth: {
            type: Number,
            default: 1,
        },
    },

    computed: {
        groupChildrenClass() {
            return [
                'query-builder-group__group-children',
                `query-builder-group__group-children--depth-${this.depth}`,
            ]
        },
        safeChildren() {
            return Array.isArray(this.group.children) ? this.group.children : []
        },
        rules() {
            return Array.isArray(this.config.rules) ? this.config.rules : []
        },
        operators() {
            return Array.isArray(this.config.operators) ? this.config.operators : []
        },
        defaultOperatorIdentifier() {
            return this.operators[0]?.identifier ?? 'all'
        },
        maxDepthExceeded() {
            return this.depth >= this.config.maxDepth
        },
        groupController() {
            return {
                rules: this.rules,
                operators: this.operators,
                currentOperator: this.group.operatorIdentifier,
                updateCurrentOperator: this.updateCurrentOperator,
                addRule: this.addRule,
                newGroup: this.newGroup,
                maxDepthExeeded: this.maxDepthExceeded,
            }
        },
    },

    created() {
        if (!this.group.operatorIdentifier) {
            this.group.operatorIdentifier = this.defaultOperatorIdentifier
        }

        if (!Array.isArray(this.group.children)) {
            this.group.children = []
        }
    },

    methods: {
        childKey(child) {
            if (!child._id) {
                child._id = nextId()
            }
            return child._id
        },
        isGroup(child) {
            return Array.isArray(child?.children)
        },
        findRule(identifier) {
            return this.rules.find(rule => rule.identifier === identifier)
        },
        defaultRuleValue(identifier) {
            const rule = this.findRule(identifier)

            return {
                value: null,
                operand: null,
                operator: Array.isArray(rule?.operators) && rule.operators.length > 0
                    ? rule.operators[0]
                    : null,
            }
        },
        addRule(identifier) {
            if (!identifier) {
                return
            }

            this.group.children.push({
                identifier,
                value: this.defaultRuleValue(identifier),
                _id: nextId(),
            })
        },
        newGroup() {
            if (this.maxDepthExceeded) {
                return
            }

            this.group.children.push(emptyGroup(this.defaultOperatorIdentifier))
        },
        removeChild(index) {
            this.group.children.splice(index, 1)
        },
        updateCurrentOperator(operatorIdentifier) {
            this.group.operatorIdentifier = operatorIdentifier
        },
        deleteLabel(child) {
            const rawLabel = this.isGroup(child)
                ? (this.config.labels?.removeGroup ?? '&times;')
                : (this.config.labels?.removeRule ?? '&times;')

            const safeLabel = String(rawLabel).replace(/<[^>]*>/g, '')
            return safeLabel === '&times;' ? '×' : (safeLabel || '×')
        }
        ruleController(child) {
            return {
                ruleIdentifier: child.identifier,
                ruleData: child.value,
                updateRuleData: newValue => {
                    child.value = {
                        ...(newValue ?? {}),
                    }
                },
            }
        },
    },
}
</script>
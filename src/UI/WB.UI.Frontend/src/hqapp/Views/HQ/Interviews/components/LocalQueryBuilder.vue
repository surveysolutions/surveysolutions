<template>
    <local-query-builder-group-node :group="query" :config="normalizedConfig" :depth="1">
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
</template>

<script>
import { isEqual } from 'lodash'
import LocalQueryBuilderGroupNode from './LocalQueryBuilderGroupNode.vue'

const defaultGroup = () => ({ operatorIdentifier: 'all', children: [] })

const cloneGroup = value => JSON.parse(JSON.stringify(value ?? defaultGroup()))

const normalizeGroup = value => {
    const group = cloneGroup(value)

    if (!group.operatorIdentifier) {
        group.operatorIdentifier = 'all'
    }

    if (!Array.isArray(group.children)) {
        group.children = []
    }

    return group
}

export default {
    name: 'LocalQueryBuilder',

    components: {
        LocalQueryBuilderGroupNode,
    },

    props: {
        config: {
            type: Object,
            required: true,
        },
        modelValue: {
            type: Object,
            required: true,
        },
    },

    emits: ['update:modelValue'],

    data() {
        return {
            query: normalizeGroup(this.modelValue),
        }
    },

    computed: {
        normalizedConfig() {
            return {
                ...this.config,
                labels: this.config?.labels ?? {},
                maxDepth: Number.isFinite(this.config?.maxDepth) ? this.config.maxDepth : Infinity,
                operators: Array.isArray(this.config?.operators) && this.config.operators.length > 0
                    ? this.config.operators
                    : [
                        { name: 'AND', identifier: 'all' },
                        { name: 'OR', identifier: 'any' },
                    ],
                rules: Array.isArray(this.config?.rules) ? this.config.rules : [],
            }
        },
    },

    watch: {
        modelValue: {
            handler(newValue) {
                const normalizedValue = normalizeGroup(newValue)

                if (!isEqual(normalizedValue, this.query)) {
                    this.query = normalizedValue
                }
            },
            deep: true,
        },
        query: {
            handler(newValue) {
                const normalizedValue = normalizeGroup(newValue)

                if (!isEqual(normalizedValue, this.modelValue)) {
                    this.$emit('update:modelValue', normalizedValue)
                }
            },
            deep: true,
        },
    },
}
</script>
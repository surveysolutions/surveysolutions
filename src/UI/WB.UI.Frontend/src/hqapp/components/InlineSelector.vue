<template>
    <span class="dropdown">
        <span role="button" style="border-bottom: 1px dashed" data-bs-toggle="dropdown" aria-haspopup="true"
            aria-expanded="true">{{ text }}</span>
        <ul class="dropdown-menu" style="width:auto">
            <li v-for="item in options" :key="item[keySelector]" @click="select(item)">
                <a href="javascript:void(0)" v-html="item[valueSelector]" />
            </li>
        </ul>
    </span>
</template>
<script>

export default {
    props: {
        options: {
            type: Array,
            required: true,
        },
        value: {
            required: true,
        },
        keySelector: {
            type: String, required: false, default: 'id',
        },
        valueSelector: {
            type: String, required: false, default: 'value',
        },
        noEmpty: {
            type: Boolean, required: false, default: false,
        },
    },

    methods: {
        select(item) {
            this.$emit('input', item)
        },
    },

    computed: {
        text() {
            if (this.value == null) {
                if (this.noEmpty && this.options.length > 0) {
                    this.select(this.options[0])
                    return ''
                }
                return this.$t('Common.SelectOption')
            }
            return this.value[this.valueSelector]
        },
    },

    watch: {
        options(to) {
            if (to[this.valueSelector] == null) {
                this.select(to[0])
            }
        },
    },

}
</script>
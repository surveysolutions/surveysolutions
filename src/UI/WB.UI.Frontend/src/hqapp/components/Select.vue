<template>
    <span class="dropdown  bootstrap-select">
        <button type="button" tabindex="-1" class="btn dropdown-toggle bs-placeholder btn-default"
            data-bs-toggle="dropdown" role="combobox" aria-owns="bs-select-1" aria-haspopup="listbox"
            aria-expanded="false" :title="text">
            <div class="filter-option">
                <div class="filter-option-inner">
                    <div class="filter-option-inner-inner">{{ text }}</div>
                </div>
            </div><span class="bs-caret"><span class="caret"></span></span>
        </button>
        <div class="dropdown-menu">
            <div class="inner" role="listbox" id="bs-select-1" tabindex="-1" aria-activedescendant="bs-select-1-0">
                <ul class="dropdown-menu inner show" role="presentation">
                    <li v-for="item in options" :key="item[keySelector]" @click="select(item)"
                        :class="{ selected: item[keySelector] == modelValue, active: item[keySelector] == modelValue }">
                        <a role="option" class="dropdown-item" id="bs-select-1-1" tabindex="0">
                            <span class="text">{{ item[valueSelector] }}</span>
                        </a>
                    </li>
                </ul>
            </div>
        </div>
    </span>
</template>
<script>


export default {
    props: {
        options: {
            type: Array,
            required: true,
        },
        modelValue: {
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
    emits: ['update:modelValue', 'change'],
    methods: {
        select(item) {
            const value = item[this.keySelector]
            this.$emit('update:modelValue', item);
            this.$emit('change', value);
        },
    },
    computed: {
        text() {
            const selOption = this.options.find(o => o[this.keySelector] == this.modelValue)
            if (selOption == null) {
                if (this.noEmpty && this.options.length > 0) {
                    this.select(this.options[0])
                    return ''
                }
                return this.$t('Common.SelectOption')
            }
            return selOption[this.valueSelector]
        },
    }
}
</script>
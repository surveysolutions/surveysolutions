<template>
    <div class="form-group" :title="tooltip">
        <input class="checkbox-filter" :id="name" type="checkbox" :disabled="!enabled ? 'disabled' : null" :name="name"
            :checked="checked" @change="checkedChange" />
        <label :for="name" :class="classes">
            <span class="tick"></span>{{ label }}</label>
    </div>
</template>

<script>
export default {
    props: {
        enabled: { type: Boolean, default: true },

        name: {
            type: String,
            required: true,
        },

        modelValue: {
            required: false,
        },
        value: {
            required: false,
        },

        label: String,

        tooltip: String,

        classes: String,
    },

    emits: ['input', 'update:modelValue'],

    methods: {
        checkedChange(ev) {
            this.$emit('input', ev.target.checked, {
                checked: ev.target.checked,
                selected: this.radioGroup,
                name: this.name,
            })
            this.$emit('update:modelValue', ev.target.checked);
        },
    },

    computed: {
        checked() {
            if (this.inputType == 'checkbox') {
                return this.modelValue !== undefined ? this.modelValue : this.value;
            }

            return this.modelValue == this.radioGroup
        },

        inputType() {
            return this.radioGroup == null ? 'checkbox' : 'radio'
        },
    },
}
</script>

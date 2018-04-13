<template>
    <div class="form-group" :title="tooltip">
        <input class="checkbox-filter" 
            :id="name + '_' + radioGroup" :type="inputType"
            :disabled="!enabled ? 'disabled': null" 
            :name="name" :checked="checked" @change="checkedChange"
            :value="radioGroup"
            >
        <label :for="name + '_' + radioGroup">
            <span class="tick"></span>{{ label }}</label>
    </div>
</template>

<script>
export default {
    props: {
        radioGroup: {
            type: String,
            default: null
        },

        enabled: {tyep: Boolean, default: true},

        name: {
            type: String,
            required: true
        },

        value: {
            required: true
        },

        label: String,

        tooltip: String
    },

    methods: {
        checkedChange(ev) {
            this.$emit("input", {
                checked: ev.target.checked,
                selected: this.radioGroup,
                name: this.name
            });
        }
    },

    computed: {
        checked() {
            if(this.inputType == 'checkbox') {
                return this.value
            } 
    
            return this.value == this.radioGroup
        },

        inputType() {
            return this.radioGroup == null ? 'checkbox' : 'radio'
        }
    }
};
</script>

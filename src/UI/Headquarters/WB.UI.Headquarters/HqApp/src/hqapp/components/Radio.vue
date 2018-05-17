<template>
    <div class="radio">
        <div class="field" :title="tooltip">
            <input class="wb-radio" type="radio"
                :id="name + '_' + radioGroup" 
                :disabled="!enabled ? 'disabled': null" 
                :name="name" :checked="checked" @change="checkedChange"
                :value="radioGroup" >
            <label :for="name + '_' + radioGroup">
                <span class="tick" style="background-position-y: 0px"></span>{{ label }}</label>
        </div>
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
}

</script>

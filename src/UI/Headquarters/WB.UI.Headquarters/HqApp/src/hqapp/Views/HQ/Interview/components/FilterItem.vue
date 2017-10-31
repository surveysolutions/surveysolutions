<template>
    <div class="form-group">
        <input class="checkbox-filter" :id="id" type="checkbox" v-model="value" @change="change">
        <label :for="id">
            <span class="tick"></span>{{title}}
        </label>
    </div>
</template>

<script>
export default {
    props: {
        id: {
            type: String,
            required: true
        },
        title: {
            type: String,
            required: true
        },
        state: {
            type: Boolean,
            default: false
        }
    },

    data() {
        return {
            value: this.state
        }
    },

    watch: {
        state(value) {
            this.value = value;
        }
    },

    methods: {
        change() {
            this.$emit("change", { id: this.id, value: this.value });
        }
    }
};
</script>

<template>
    <div class="form-group">
        <input class="checkbox-filter"
            :id="option"
            type="checkbox"
            v-model="value"
            @change="change">
        <label :for="option">
            <span class="tick"></span>{{title}} ({{ found }})
        </label>
    </div>
</template>

<script>
export default {
    props: {
        option: {
            type: String,
            required: true,
        },
        title: {
            type: String,
            required: true,
        },
    },

    data() {
        return {
            value: '',
        }
    },

    watch: {
        state(value) {
            this.value = value
        },
    },

    created(){
        this.value = this.state
    },

    methods: {
        change() {
            this.$emit('change', { id: this.option, value: this.value })
        },
    },

    computed: {
        found() {
            return this.$store.state.review.filters.stats[this.option]
        },
        state() {
            return  this.$store.getters.filteringState[this.option]
        },
    },
}
</script>

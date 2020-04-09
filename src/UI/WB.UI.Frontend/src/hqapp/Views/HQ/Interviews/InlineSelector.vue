<template>
    <span class="dropdown">
        <span role="button"
            style="border-bottom: 1px dashed"
            data-toggle="dropdown"
            aria-haspopup="true"
            aria-expanded="true">
            {{value == null ? 'Select question' : value[valueSelector]}}
        </span>
        <ul class="dropdown-menu"
            style="width:auto">
            <li v-for="item in options"
                :key="item[keySelector]"               
                @click="select(item)" >
                <a href="#"
                    v-html="item[valueSelector]" /></li>
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
    },

    methods: {
        select(item) {
            this.$emit('input', item)
        },
    },

    watch: {
        options(to) {
            if(to[this.valueSelector] == null) {
                this.select(to[0])
            }
        },
    },

}
</script>
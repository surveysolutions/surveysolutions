<template>
    <div class="input-group">
        <input class="form-control with-clear-btn" :placeholder="placeholder" :type="type" 
                :value="value" @change="changed"/>
        <div class="input-group-btn" @click="clearFilter">
            <div class="btn btn-default">
                <span class="glyphicon glyphicon-remove" aria-hidden="true"></span>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    props: {
        value: {
            default: null       
        },
        type: {
            type: String,
            default: "text"
        },
        placeholder: {
            type: String,
            default: null
        }
    },
    methods: {
        clearFilter(){
            this.$emit("input", null)
            this.$emit("changed")
        },
        changed(ev) {
            if(ev != null && ev.srcElement == null) return
            this.$emit("input", parseInt(ev.srcElement.value))
            this.$emit("changed")
        }
    }
};
</script>

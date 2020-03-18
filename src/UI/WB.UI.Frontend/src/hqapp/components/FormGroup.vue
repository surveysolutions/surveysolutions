<template>
    <div class="block-filter">
        <label v-if="label !== undefined"
            :class="{ 'compulsory-field' : mandatory === true }">
            {{ label }}
        </label>
        <div class="form-group"
            :class="{'has-error': error !== undefined}">
            <slot></slot>
            <span v-if="error !== undefined"
                class="help-block field-validation-error">
                <span>{{ error }}</span>
            </span>
        </div>
    </div>
</template>

<script>
export default {
    props: ['label', 'error', 'mandatory'],
}
</script>

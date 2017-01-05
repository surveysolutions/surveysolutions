<template>
    <div class="question">
        <h5>Lorem ipsum dolor sit amet, consectetur adipiscing elit <small>Text question</small></h5>
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field answered">
                        <input type="number" class="field-to-fill" placeholder="Tap to enter number" value="6">
                        <button type="submit" class="btn btn-link btn-clear">
                            <span></span>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'TextQuestion'
    }
</script>

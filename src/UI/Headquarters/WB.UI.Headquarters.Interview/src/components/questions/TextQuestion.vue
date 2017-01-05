<template>
    <div class="question">
        <div class="question-editor text-question">
            <h5>
                Text question basic template <small>{{entity.Identity}}</small>
            </h5>
            <div class="question-unit">
                <div class="options-group">
                    <div class="form-group">
                        <div class="field answered">
                            <input type="text" class="field-to-fill" placeholder="Tap to enter number" value="Selling groceries">
                            <button type="submit" class="btn btn-link btn-clear">
                                <span></span>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'TextQuestion',
        props: ['entity']
    }
</script>

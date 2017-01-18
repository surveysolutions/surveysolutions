<script lang="ts">
    import * as bootbox from "bootbox"

    export default {
        methods: {
            confirm: function(message, callback) {
                bootbox.confirm(message, callback)
            }
        }
    }

</script>


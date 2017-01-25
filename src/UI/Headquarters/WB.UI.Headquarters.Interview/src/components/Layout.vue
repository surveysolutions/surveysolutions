<template>
    <div id="root">
        <header>
            <slot name="header"></slot>
        </header>
        <main class="web-interview hold-transition">
            <div class="container-fluid">
                <slot></slot>
            </div>
        </main>
        <footer>
            <slot name="footer"></slot>
        </footer>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'wb-layout'
    }
</script>
